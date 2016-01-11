using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace BasicWebsite {
    public class RecordingsReader {
        public static List<RecordingItem> Recordings { get; set; }

        public static void InitFromFile() {
            var fileLines  = File.ReadAllLines( HttpContext.Current.Server.MapPath( "Data.txt" ) );
            string[] reads = new string[] { "Четец:", "Чете:", "Прочит:", "четец:" };
            string[] otherBeginings = new string[] { "Издателство/година на издаване–създаване:", "Издателство/година на издаване/създаване:", "Брой файлове:", "Прослушване:", "Изд.", "Издателство" };
            string[] startOfList = new string[] { "„", "\"", "«" };

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
                        if ( line4.ToLowerInvariant().StartsWith( "от" ) ) {
                            item.FromBook = line4;
                        } else {
                            item.Category = line4;
                        }
                        
                        i += 3;
                    } else if ( startOfList.Any(x=> line4.StartsWith( x ))) {
                        item.Author = line2;
                        item.Title = line3;

                        var j = 3;
                        var notesBuilder = new StringBuilder();
                        var newline = line4;
                        while ( startOfList.Any( x => newline.StartsWith( x ) ) ) {
                            notesBuilder.AppendLine( newline );
                            j++;
                            newline = fileLines[i + j];
                        }
                        item.ItemList = notesBuilder.ToString();
                        i += j;
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
                    "добавена в библиотеката:",
                    "Добавено в каталога:".ToLowerInvariant()
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
                //var sources = new string[] { "източник :" };
                if ( line.ToLowerInvariant().StartsWith( "Източник".ToLowerInvariant() ) ) {
                    item.Source = line.Substring( line.IndexOf( ":" ) ).Trim( ' ', ':' );
                    continue;
                }
                if ( line.StartsWith( "Брой файлове:" ) ) {
                    item.FilesCount = line.Substring( line.IndexOf( ":" ) ).Trim( ' ', ':' );
                    continue;
                }
                if ( line.StartsWith( "Забележки" ) ) {
                    item.Notes = line.Substring( line.IndexOf( ":" ) ).Trim( ' ', ':' );
                    continue;
                }
                var publishingStrings = new string[]{
                    "Издателство/година на издаване/създаване:",
                    "Издателство/година на издаване–създаване:",
                    "Изд.",
                    "Издателство"
                    
                };
                if ( publishingStrings.Any( x => line.StartsWith( x ) ) ) {
                    var index = line.IndexOf( ":" ) >= 0 ? line.IndexOf( ":" )
                        : line.IndexOf( "." ) >= 0 ? line.IndexOf( "." ) : line.IndexOf( " " );
                    item.PublishingHouseYear = line.Substring( index ).Trim( ' ', ':', '.' );
                    continue;
                }
                if ( line.StartsWith( "http" ) ) {
                    item.Url = line;
                    continue;
                }
                notFound.Add( line );
            }
            Recordings = list;
        }

        public static void InitVersion2() {
            var fileLines = File.ReadAllLines( HttpContext.Current.Server.MapPath( "Data.txt" ) );

            RecordingItem item = new RecordingItem();
            var notFound = new List<string>();

            //var newlineIndexes = fileLines)

            var list = new List<RecordingItem>();

            var newLines = fileLines.IndexesWhere( x => String.IsNullOrWhiteSpace( x ) ).ToList();

            var data = new List<List<string>>();

            var current = 0;
            foreach ( var line in newLines ) {
                data.Add( fileLines.Skip( current ).Take( line - current ).Where(x=>!String.IsNullOrWhiteSpace(x)).ToList() );
                current = line;
            }

            FileData = data.Select( x => new FileDataItem() {
                Lines = x,
                Number = Int32.Parse( x[0].Trim( '.', ' ' ) )
            } ).ToList();
        }

        public static List<FileDataItem> FileData { get; set; }
    }

    public static class Extensions {
        public static IEnumerable<int> IndexesWhere<T>( this IEnumerable<T> source, Func<T, bool> predicate ) {
            int index = 0;
            foreach ( T element in source ) {
                if ( predicate( element ) ) {
                    yield return index;
                }
                index++;
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

        public string Url { get; set; }

        public string ItemList { get; set; }

        public string FromBook { get; set; }
    }

    public class FileDataItem {
        public int Number { get; set; }
        public List<string> Lines { get; set; }
    }

}