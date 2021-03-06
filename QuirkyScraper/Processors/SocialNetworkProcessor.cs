﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace QuirkyScraper.Processors
{
    class SocialNetworkProcessor : Processor
    {
        private string rawFilePath;

        public SocialNetworkProcessor(string rawFilePath)
        {
            this.rawFilePath = rawFilePath;
        }

        protected override string DEFAULT_SAVE_PATH
        {
            get { return @"SocialNetworkResults"; }
        }

        class SimplifiedPeople: List<SimplifiedPerson>
        {
            public new void AddRange(IEnumerable<SimplifiedPerson> range)
            {
                foreach (SimplifiedPerson person in range)
                    Add(person);
            }

            public new void Add(SimplifiedPerson person)
            {
                if (this.Count <= 0)
                {
                    base.Add(person);
                    return;
                }

                for(int i = 0; i < this.Count; i++)
                {
                    if(person.CompareTo(this[i]) <= 0)
                    {
                        base.Insert(i, person);
                        return;
                    }
                }

                base.Add(person);
            }
        }

        class SimplifiedPerson: IEquatable<SimplifiedPerson>, IComparable<SimplifiedPerson>
        {
            public List<SimplifiedPerson> Connections { get; internal set; }
            public string Name { get; set; }

            public int CompareTo(SimplifiedPerson other)
            {
                return this.Name.ToLower().CompareTo(other.Name.ToLower());
            }

            public bool Equals(SimplifiedPerson other)
            {
                return string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
            }
        }

        class SimplifiedPeopleComparer : IEqualityComparer<SimplifiedPerson>
        {
            public bool Equals(SimplifiedPerson x, SimplifiedPerson y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(SimplifiedPerson people)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(people, null)) return 0;

                //Get hash code for the Name field if it is not null.
                int hashProductName = people.Name == null ? 0 : people.Name.GetHashCode();

                //Get hash code for the Code field.
                int hashProductCode = people.Connections.GetHashCode();

                //Calculate the hash code for the product.
                return hashProductName ^ hashProductCode;
            }
        }

        public override void Process()
        {
            if (!Directory.Exists(Savepath))
                Directory.CreateDirectory(Savepath);

            List<SimplifiedPerson> graph = BuildGraph();
            SaveGraph(graph);
            //GenerateGraph();
            MessageBox.Show("Social network have been generated in " + Savepath);
        }

        private void SaveGraph(List<SimplifiedPerson> graph)
        {
            var totalCount = graph.Count * 2 * 100 / 75;
            var count = totalCount * 25 / 100;  // Start from 25%
            ReportProgress(count, totalCount);

            var sizeX = 1000;
            var sizeY = 1000;

            var rowBlocks = (graph.Count / sizeY) + 1;
            var colBlocks = (graph.Count / sizeX) + 1;
            for (int i = 0; i < rowBlocks; i++)
            {
                for (int j = 0; j < colBlocks; j++)
                {
                    var dimName = i + "_" + j;
                    var filePath = Path.Combine(Savepath, ("SocialNetwork_" + dimName).RemoveInvalidFilePathCharacters() + ".xls");
                    XmlWriter writer = Helper.GenerateXmlWriter(filePath);
                    writer.StartCreateXls()
                        .CreateWorksheet("Social Network " + dimName);

                    var startRowIndex = i * sizeY;
                    var startColIndex = j * sizeX;

                    // Setup header
                    // Creates a row.
                    writer.CreateRow()
                        .WriteCell(string.Empty);
                    for (var index = startColIndex; index < startColIndex + sizeX; index++)
                    {
                        if (index >= graph.Count)
                            break;
                        else
                            writer.WriteCell(graph[index].Name, true);
                    }
                    writer.CloseRow();

                    // Populate data
                    for (var row = 0; row < sizeY; row++)
                    {
                        var rowIndex = row + startRowIndex;
                        if (rowIndex >= graph.Count) break;

                        writer.CreateRow()
                            .WriteCell(graph[rowIndex].Name, true);

                        for (var col = 0; col < sizeX; col++)
                        {
                            var colIndex = col + startColIndex;
                            if (colIndex >= graph.Count) break;

                            if (row == col)
                            {
                                writer.WriteCell(string.Empty);
                            }

                            bool related = graph[row].Connections.Contains(graph[col]);
                            if (related)
                                writer.WriteCell("1");
                            else
                                writer.WriteCell("0");
                        }
                        writer.CloseRow();

                        ReportProgress(++count, totalCount);
                    }

                    writer.CloseWorksheet()
                        .CloseXls()
                        .Close();
                }
            }
        }

        private List<SimplifiedPerson> BuildGraph()
        {
            List<SimplifiedPerson> graph = new List<SimplifiedPerson>();
            var filePath = Helper.CloneTextFileAndAddArray(this.rawFilePath);
            using (TextReader textReader = new StreamReader(filePath))
            using (var reader = new JsonTextReader(textReader))
            {
                JsonSerializer cachedDeserializer = JsonSerializer.Create();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        People deserializedItem = cachedDeserializer.Deserialize<People>(reader);

                        if (string.IsNullOrWhiteSpace(deserializedItem.Name)) continue;

                        SimplifiedPeopleComparer comparer = new SimplifiedPeopleComparer();
                        SimplifiedPerson sp = new SimplifiedPerson
                        {
                            Name = deserializedItem.Name
                        };
                        if (graph.Contains(sp, comparer)) continue;

                        // Get this persons connections
                        List<SimplifiedPerson> connections = new List<SimplifiedPerson>();
                        if (deserializedItem.Followers != null)
                            connections.AddRange(deserializedItem.Followers.Select(x => new SimplifiedPerson { Name = x.Name }).Distinct());

                        if (deserializedItem.Followings != null)
                            connections.AddRange(deserializedItem.Followings.Select(x => new SimplifiedPerson { Name = x.Name }).Distinct());

                        connections = connections.Distinct().ToList();  // Ensure no duplicates

                        sp.Connections = connections;

                        graph.Add(sp);
                    }
                }
            }

            return graph;
        }

        private void GenerateGraph()
        {
            ReportProgress(0, "Retrieving people details");
            List<IPeople> people = GetPeople();

            ReportProgress(20, "Preparing...");
            // Get distinct people
            IEqualityComparer<IPeople> comparer = new PeopleComparer();
            List<IPeople> distinctPeople = people.Concat(people.SelectMany(x => x.Followers).Distinct(comparer).Concat(people.SelectMany(x => x.Followings)).Distinct(comparer)).Distinct(comparer).ToList();

            var totalCount = distinctPeople.Count * 2 * 100/ 75;
            var count = totalCount * 25 / 100;  // Start from 25%
            ReportProgress(count, totalCount);

            // Build grid
            var grid = new int[distinctPeople.Count, distinctPeople.Count];
            for(var row = 1; row < grid.GetLength(0); row++)    // starts from 1 as we do not record relationship with oneself
            {
                var contributor = people.FirstOrDefault(x => comparer.Equals(x, distinctPeople[row]));
                if (contributor == null) continue;  // The current row person did not contribute. Others will fill his relationships

                for (var col = 0; col < row; col++)  // Bottom triangle is the same as top triangle
                {
                    if (row == col) continue;   // No need to record relationship with oneself

                    bool related = contributor.Followers.Contains(distinctPeople[col], comparer)
                        || contributor.Followings.Contains(distinctPeople[col], comparer);
                    int mark = related ? 1 : 0;
                    grid[row, col] = grid[col, row] = mark;
                }
                ReportProgress(++count, totalCount);
            }

            var sizeX = 1000;
            var sizeY = 1000;

            var rowBlocks = (grid.GetLength(0) / sizeY) + 1;
            var colBlocks = (grid.GetLength(1) / sizeX) + 1;
            for (int i = 0; i < rowBlocks; i++)
            {
                for (int j = 0; j < colBlocks; j++)
                {
                    var dimName = i + "_" + j;
                    var filePath = Path.Combine(Savepath, ("SocialNetwork_" + dimName).RemoveInvalidFilePathCharacters() + ".xls");
                    XmlWriter writer = Helper.GenerateXmlWriter(filePath);
                    writer.StartCreateXls()
                        .CreateWorksheet("Social Network " + dimName);

                    var startRowIndex = i * sizeY;
                    var startColIndex = j * sizeX;

                    // Setup header
                    // Creates a row.
                    writer.CreateRow()
                        .WriteCell(string.Empty);
                    for (var index = startColIndex; index < startColIndex + sizeX; index++)
                    {
                        if (index >= distinctPeople.Count)
                            break;
                        else
                            writer.WriteCell(distinctPeople[index].Name, true);
                    }
                    writer.CloseRow();

                    // Populate data
                    for (var row = 0; row < sizeY; row++)
                    {
                        var rowIndex = row + startRowIndex;
                        if (rowIndex >= grid.GetLength(0)) break;

                        writer.CreateRow()
                            .WriteCell(distinctPeople[rowIndex].Name, true);

                        for (var col = 0; col < sizeX; col++)
                        {
                            var colIndex = col + startColIndex;
                            if (colIndex >= grid.GetLength(1)) break;

                            writer.WriteCell(grid[rowIndex, colIndex].ToString());
                        }
                        writer.CloseRow();

                        ReportProgress(++count, totalCount);
                    }

                    writer.CloseWorksheet()
                        .CloseXls()
                        .Close();
                }
            }
        }

        private List<IPeople> GetPeople()
        {
            if (!File.Exists(this.rawFilePath)) return new List<IPeople>();

            try
            {
                return Helper.GetJsonIEnumerableFromTemp<People>(this.rawFilePath).Cast<IPeople>().ToList();
            }
            catch
            {   // If some how file is corrupted/unreadable get new list
                return new List<IPeople>();
            }
        }
    }
}
