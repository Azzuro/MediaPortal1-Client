using System;
using System.Collections;
using System.IO;
using MediaPortal.GUI.Library;

namespace GUIRecipies
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class RecipieReader
	{
		string[] unit = {"x ","sm","fl","c ","pt","md","lg","cn","pk","pn","dr","ds","ct",
                         "bn","sl","ea","t ","ts","T ","tb","qt","ga","oz","lb","ml","cb",
                         "cl","dl","l ","mg","cg","dg","g ","kg"};

		protected string mFileName;

		public int RecipieCount
		{
			get { return recipieCount; }
		}

		protected int recipieCount = 0;

		public RecipieReader( string fileName )
		{
			mFileName = fileName;
		}

		public void GetRecipies()
		{
			StreamReader reader = new StreamReader( mFileName, System.Text.Encoding.Default );
			string contents = "";
			string line = reader.ReadLine();
			line = reader.ReadLine();
			while( line != null )
			{
				//Console.WriteLine( line );
				
				contents = contents + "\n" + line;

				//Check if this is the end of the recipie
				if( line.Equals( "MMMMM" ) || line.Equals( "-----" ))
				{
					recipieCount++;
					BuildRecipie( contents );
					contents = "";
					line= " ";
					while (line != null && !line.Trim().StartsWith( "MMMMM" ) && !line.Trim().StartsWith( "-----" ))
					{
						line = reader.ReadLine();
					}
				}
				line = reader.ReadLine();
			}
			Console.WriteLine( "Read " + recipieCount + " recipies." );
			reader.Close();
		}

		void BuildRecipie( string contents )
		{
			string[] lines = contents.Split( '\n' );
			
			Recipie rec = new Recipie();
			string rest = "";
			string ing = "";
			string tit = "";
			int num = 0;
			int s1 = 0;
			int s2 = 0;
			int rem = 0;
			bool catTst=false;

			foreach( string line in lines )
			{
				if( line.Trim().StartsWith( "Title: " ) )				// Read Title
				{
					tit=line.Trim().Substring( 7 );
					rec.Title = tit.TrimEnd();
				}
				else if( line.Trim().StartsWith( "Categories: " ) )		// Read Categorie
				{
					catTst=true;
					tit=line.Trim().Substring( 12 );
					rec.SetCategories( tit.TrimEnd());
				}
				else if( line.Trim().StartsWith( "Yield: " ) )			//  Read Yield
				{
					rec.Yield = line.Trim().Substring( 7 );
					rec.CYield = 0;
				}														
				else if( line.Trim().StartsWith( "Servings: " ) )		//  Read Servings
				{
					rec.Yield = line.Trim().Substring( 9 );
					rec.CYield = 0;
				}														// Read Remarks
				else if( line.Trim().StartsWith( ":" ) || line.Trim().StartsWith( "," ))
				{
					rec.AddRemarks (line.Trim().Substring( 1 ));
				}
				else if( line.Trim().StartsWith( "*  Quelle:" ) )		// Read Remarks
				{
					rec.AddRemarks (line.Trim().Substring( 1 ));
					rem=1;
				}
				else
				{
					char[] chars = line.ToCharArray();
					if( line.Trim().Length > 0 && !line.Trim().StartsWith( "MMMMM" ) && !line.Trim().StartsWith( "-----" )&& rem==0 )
					{
						if( chars.Length > 11 && chars[ 0 ] == ' ' && chars[ 7 ] == ' ' && chars[ 10 ] == ' ')
						{
							if (chars[11] == '-')
							{
								num=1;
								chars[11] = ' ';
							} 
							else 
							{
								ing = line.Substring(8,2);
								num=0;
								for (int i=0; i<=32; i++) 
								{ 
									if (unit[i] == ing) 
									{
										num = i + 2015;
									}
								}
							}
							rec.AddUnit( Convert.ToString( num )); 
							rec.AddLot( line.Substring(0,7) );
							rec.AddIngredient( line.Substring(10) );
						}
						else
						{
							rest = rest + line.Trim()+" ";
						}
					} 
					else 
					{
						if (line.Trim().StartsWith("MMMMM-") || line.Trim().StartsWith("------")) 
						{
							rec.AddUnit("2");
							rec.AddLot(" ");
							s1=5;
							s2=6;
							for (int i=5; i < chars.Length; i++) 
							{
								if (chars[i] != '-') 
								{
									s1=i; 
									break;
								}
							}
							for (int i=s1+1; i<chars.Length; i++) 
							{
								if (chars[i] == '-') 
								{
									s2=i; 
									break;
								}
							}
							rec.AddIngredient( "\n"+line.Trim().Substring(s1,(s2-s1)) );
						} 
						else 
						{
							if (line.Trim().StartsWith("MMMMM") || line.Trim().StartsWith("-----")) rem=0;

						}
					}
				}
			}
			rec.Directions = rest;
			if (catTst==true) RecipieDatabase.GetInstance().AddRecipie( rec );
		}
	}
}
