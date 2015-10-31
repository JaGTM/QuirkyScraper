using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace QuirkyScraper
{
    class GenerateSocialNetworkProcessor : Processor
    {
        private string rawFilePath;

        public GenerateSocialNetworkProcessor(string rawFilePath)
        {
            this.rawFilePath = rawFilePath;
        }

        protected override string DEFAULT_SAVE_PATH
        {
            get { return @"D:\Users\JaG\Desktop\SocialNetworkResults\"; }
        }

        public override void Process()
        {
            if (!Directory.Exists(Savepath))
                Directory.CreateDirectory(Savepath);

            GenerateGraph();
            MessageBox.Show("Social network have been generated in " + Savepath);
        }

        private void GenerateGraph()
        {
            List<IPeople> people = GetPeople();

            var count = 0;

            // Get distinct people
            IEqualityComparer<IPeople> comparer = new PeopleComparer();
            List<IPeople> distinctPeople = people.Concat(people.SelectMany(x => x.Followers).Distinct(comparer).Concat(people.SelectMany(x => x.Followings)).Distinct(comparer)).Distinct(comparer).ToList();

            var totalCount = distinctPeople.Count * 2;
            ReportProgress(count, totalCount);

            // Build grid
            var grid = new int[distinctPeople.Count, distinctPeople.Count];
            for(var row = 1; row < grid.GetLength(0); row++)    // starts from 1 as we do not record relationship with oneself
            {
                var contributor = people.FirstOrDefault(x => comparer.Equals(x, distinctPeople[row]));
                if (contributor == null) continue;  // The current row person did not contribute. Others will fill his relationships

                for (var col = 0; col < row; col++)  // Bottom triangle is the same as top triangle
                {
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
