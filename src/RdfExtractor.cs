/***************************************************************************
 *  RdfExtractor.cs
 *
 *  Copyright (C) 2006 Luke Hoersten
 *  Written by Luke Hoersten <luke.hoersten@gmail.com>
 ****************************************************************************/

using System;
using System.Net;
using System.IO;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;

namespace CreativeCommons
{
	public class RdfExtractor
	{
	    private Stream stream;
	    		public RdfExtractor(string uri)
		{
		    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			stream = response.GetResponseStream();
		}

		public RdfExtractor(Stream stream)
		{
			this.stream = stream;
		}

		public string ParseRdf()
		{
		    Regex expression = new Regex(@"(\<rdf:RDF xmlns=""http://web.resource.org/cc/""[\s\S]{0,}?\/rdf:RDF\>)");
		    StreamReader reader = new StreamReader(stream);
			MatchCollection matches = expression.Matches(reader.ReadToEnd());
			
			StringBuilder result = new StringBuilder();
			foreach(Match line in matches) {
				result.Append(line.Value);
			}
			
			return result.ToString();
		}
			
        public static void Main(string[] args)
        {
            if(args.Length < 1) {
                Console.WriteLine("Must give name of file(s) to parse Creative Commons RDF metadata from.");
                return;
            }
            
            foreach(string uri in args) {
                RdfExtractor parser = new RdfExtractor(uri);
                Console.WriteLine("File: \"{0}\" RDF: \"{1}\"", uri, parser.ParseRdf());
                Console.WriteLine("\n==================\n");
            }
        }
	}
}
	
