using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace BasicWebsite {
    public class RecordingsReader {
        public static List<RecordingItem> Recordings { get; set; }

        public static void InitFromFile() {
            var fileLines  = File.ReadAllLines( HttpContext.Current.Server.MapPath( "Data.txt" ) );
            string[] reads = new string[] { "Четец:", "Чете:", "Прочит:" };
            string[] otherBeginings = new string[] { "Издателство/година на издаване–създаване:", "Брой файлове:", "Прослушване:" };

            RecordingItem item = new RecordingItem();
            var notFound = new List<string>();

            //var newlineIndexes = fileLines)

            var list = new List<RecordingItem>();
            for ( var i = 0; i < fileLines.Length; i++ ) {
                var line = fileLines[i];
                //first line
                if ( line.Trim(' ').EndsWith( "." ) && line.Length <= 5 ) {
                    var value = line.Trim( '.', ' ' );
                    item.Number = Int32.Parse( value );
                    var line2 = fileLines[i + 1];
                    var line3 = fileLines[i + 2];
                    var line4 = fileLines[i + 3];
                    var line5 = fileLines[i + 4];
                    if ( reads.Any( x => line3.StartsWith( x ) ) || otherBeginings.Any( x => line3.StartsWith( x ) ) ) {
                        item.Title = line2;
                        i++;
                    } else if ( reads.Any( x => line4.StartsWith( x ) ) || otherBeginings.Any( x => line4.StartsWith( x ) )) {
                        item.Author = line2;
                        item.Title = line3;
                        i += 2;
                    } else if ( reads.Any( x => line5.StartsWith( x ) ) || otherBeginings.Any( x => line5.StartsWith( x ) )) {
                        item.Author = line2;
                        item.Title = line3;
                        item.Category = line4;
                        i += 3;
                    }
                    continue;
                }
                
                //last line
                if ( string.IsNullOrWhiteSpace( line ) ) {
                    list.Add( item );
                    item = new RecordingItem();
                    continue;
                }
                
                if ( reads.Any( x => line.StartsWith( x ) ) ) {
                    item.ReadBy = line.Substring( line.IndexOf( ":" ) ).Trim( ' ',':' );
                    continue;
                }
                var addedToLib = new string[]{
                    "Добавено в Библиотеката:".ToLowerInvariant(),
                    "Добавенa в Библиотеката:".ToLowerInvariant(),
                    "добавена в библиотеката:"
                };
                var lowLine = line.ToLowerInvariant();
                if ( addedToLib.Any(x=>lowLine.StartsWith(x)) ) {
                    item.AddedToLibString = line.Substring( line.IndexOf( ":" ) ).Trim( ' ', ':' );
                    continue;
                }
                if ( line.StartsWith( "Прослушване:" ) ) {
                    item.ListenedBy = line.Substring( line.IndexOf( ":" ) ).Trim( ' ', ':' );
                    continue;
                }
                if ( line.StartsWith( "Монтаж:" ) ) {
                    item.Installation = line.Substring( line.IndexOf( ":" ) ).Trim( ' ', ':' );
                    continue;
                }
                if ( line.StartsWith( "Прослушване/монтаж:" ) ) {
                    item.ListenedBy = item.Installation = line.Substring( line.IndexOf( ":" ) ).Trim( ' ', ':' );
                    continue;
                }
                if ( line.StartsWith( "Източник:" ) ) {
                    item.Source = line.Substring( line.IndexOf( ":" ) ).Trim( ' ', ':' );
                    continue;
                }
                if ( line.StartsWith( "Брой файлове:" ) ) {
                    item.FilesCount = line.Substring( line.IndexOf( ":" ) ).Trim( ' ', ':' );
                    continue;
                }
                if ( line.StartsWith( "Забележки:" ) ) {
                    item.Notes = line.Substring( line.IndexOf( ":" ) ).Trim( ' ', ':' );
                    continue;
                }
                var publishingStrings = new string[]{
                    "Издателство/година на издаване/създаване:",
                    "Издателство/година на издаване–създаване:",
                    
                };
                if ( publishingStrings.Any( x => line.StartsWith( x ) ) ) {
                    item.PublishingHouseYear = line.Substring( line.IndexOf( ":" ) ).Trim( ' ', ':' );
                    continue;
                }
                notFound.Add( line );
            }
        }

    }

    public class RecordingItem {
        public int Number { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }
        public string ReadBy { get; set; }
        public string AddedToLibString { get; set; }
        public string Installation { get; set; }

        public string Category { get; set; }

        public string ListenedBy { get; set; }

        public string Source { get; set; }

        public string FilesCount { get; set; }

        public string PublishingHouseYear { get; set; }

        public string Notes { get; set; }
    }


}