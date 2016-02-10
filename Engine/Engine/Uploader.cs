using System;
using System.Data.SqlClient;
using System.Xml;

namespace Engine
{
    public class Uploader
    {
        String TreeFile;
        Attr[] Attributes;
        String Type;
        String SplitSize;
        String Depth;
        SqlConnection MyConnection;

        public Uploader()
        {

            //Connection to DB.
            SqlConnector SqlConn = new SqlConnector();
            this.MyConnection = new SqlConnection("user id=" + SqlConn.Username + ";" +
                                           "password=" + SqlConn.Password + ";server=" + SqlConn.Server + ";" +
                                           "Trusted_Connection=yes;" +
                                           "database=" + SqlConn.Database + "; " +
                                           "connection timeout=30");
            try
            {
                this.MyConnection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }

        public void SaveToDB(String treeFile)
        {

            //Adding the file path to the parameters of the class.
            this.TreeFile = treeFile;

            //Initializing the XMLTree object that contains all files.
            XmlDocument xmlTree = new XmlDocument();

            //Uploading Tree file.
            xmlTree.Load(treeFile);

            //Depth node.
            int depth = 0;

            //Opening "Informations" node and extrapolate the info (Type, SplitSize e Depth).
            XmlNode InfoNode = xmlTree.DocumentElement.SelectSingleNode("Informations");
            this.Type = InfoNode.SelectSingleNode("Type").InnerText;
            this.SplitSize = InfoNode.SelectSingleNode("SplitSize").InnerText;
            this.Depth = InfoNode.SelectSingleNode("Depth").InnerText;


            //Generating array with Attributes.
            XmlNodeList AttrList = InfoNode.SelectSingleNode("Attributes").SelectNodes("Attribute");
            this.Attributes = new Attr[AttrList.Count];

            //Inserting elements in to array and query to insert AttrDef.
            for (int j = 0; j < AttrList.Count; j++)
            {

                this.Attributes[j] = new Attr(AttrList.Item(j).SelectSingleNode("Type").InnerText, AttrList.Item(j).SelectSingleNode("Name").InnerText);
                String AttrDefUID = "";

                //Inserting attributes in to DB.
                try
                {
                    SqlDataReader myReader = null;
                    SqlCommand myCommand = new SqlCommand("INSERT INTO AttrDef OUTPUT Inserted.AttrDefUid VALUES (newid(), '" + Attributes[j].Name + "');", this.MyConnection);
                    myReader = myCommand.ExecuteReader();

                    while (myReader.Read())
                    {
                        AttrDefUID = myReader["AttrDefUid"].ToString();

                    }
                    myReader.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                this.Attributes[j].AttributeUID = AttrDefUID;

            }

            // Vertex Root.
            XmlNode RootVertex = xmlTree.DocumentElement.SelectSingleNode("Vertex");

            //Vertex Name.
            String VertexName = RootVertex.SelectSingleNode("Name").InnerText;

            String vertexUID = "";

            try
            {
                SqlDataReader myReader = null;
                SqlCommand myCommand = new SqlCommand("INSERT INTO Vertex (VertexUid, Name, Type, Depth) OUTPUT Inserted.VertexUid VALUES (newid(), '" + VertexName + "', '" + this.Type + "', " + depth + ");",
                                                         this.MyConnection);
                myReader = myCommand.ExecuteReader();

                while (myReader.Read())
                {
                    vertexUID = myReader["VertexUid"].ToString();

                }

                myReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            XmlNode AttributesNode = RootVertex.SelectSingleNode("Attributes");

            //Adding Vertex Attributes to DB.
            for (int i = 0; i < Attributes.Length; i++)
            {

                XmlNode SingleAttr = AttributesNode.SelectSingleNode(Attributes[i].Name);

                if (SingleAttr != null)
                {
                     try
                    {
                        SqlDataReader myReader = null;
                        SqlCommand myCommand = new SqlCommand("INSERT INTO AttrUsage VALUES (newid(), '" + vertexUID + "', '" + Attributes[i].AttributeUID + "', '" + SingleAttr.InnerText + "')", this.MyConnection);
                        myReader = myCommand.ExecuteReader();

                        myReader.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                }
               
            }

            this.SaveToDB(vertexUID, RootVertex, depth + 1);

        }

        private void SaveToDB(String RootUid, XmlNode RootVertex, int depth)
        {

            // Start recursion.
            int intSplitSize = 0;
            int.TryParse(this.SplitSize, out intSplitSize);

            for (int i = 0; i < intSplitSize; i++)
            {
                //Finding edge at the current level.
                XmlNode EdgeNode = RootVertex.SelectSingleNode("Edge[" + (i + 1) + "]");

                if (EdgeNode != null)
                {

                    String EdgeUID = "";
                    //Inserting Edge.
                    try
                    {
                        SqlDataReader myReader = null;
                        //Inserting Edge into Edge Table.
                        SqlCommand myCommand = new SqlCommand("INSERT INTO Edge (EdgeUid, StartVertexUid) OUTPUT Inserted.EdgeUid VALUES (newid(), '" + RootUid + "');", this.MyConnection);
                        myReader = myCommand.ExecuteReader();

                        while (myReader.Read())
                        {
                            // Returning EdgeUid with trigger.
                            EdgeUID = myReader["EdgeUid"].ToString();
                           
                        }
                        myReader.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                    XmlNode AttributesNode = EdgeNode.SelectSingleNode("Attributes");

                    //Adding Edge Attributes to DB.
                    for (int j = 0; j < Attributes.Length; j++)
                    {

                        XmlNode SingleAttr = AttributesNode.SelectSingleNode(Attributes[j].Name);
                        if (SingleAttr != null)
                        {
                            try
                            {
                                SqlDataReader myReader = null;
                                SqlCommand myCommand = new SqlCommand("INSERT INTO AttrUsage VALUES (newid(), '" + EdgeUID + "', '" + Attributes[j].AttributeUID + "', '" + SingleAttr.InnerText + "')", this.MyConnection);
                                myReader = myCommand.ExecuteReader();

                                myReader.Close();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }

                        }
                    }

                    //Analyze the vertex connected to edge just entered and inserting it in the DB.
                    XmlNode SubVertex = EdgeNode.SelectSingleNode("Vertex");

                    String VertexName = SubVertex.SelectSingleNode("Name").InnerText;
                    String VertexUid = "";

                    try
                    {
                        SqlDataReader myReader = null;
                        //Insert Vertex into Vertex table.
                        SqlCommand myCommand = new SqlCommand("INSERT INTO Vertex OUTPUT Inserted.VertexUid VALUES (newid(), '" + VertexName + "', '" + this.Type + "', " + depth + ", '" + EdgeUID + "') ; ", this.MyConnection);
                        myReader = myCommand.ExecuteReader();

                        while (myReader.Read())
                        {
                            VertexUid = myReader["VertexUid"].ToString();

                        }
                        myReader.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                    // Update the edge table connecting the vertex just entered.
                    try
                    {
                        SqlDataReader myReader = null;
                        SqlCommand myCommand = new SqlCommand("UPDATE Edge SET EndVertexUid = '" + VertexUid + "' WHERE  EdgeUid = '" + EdgeUID + "';", this.MyConnection);
                        myReader = myCommand.ExecuteReader();
                        myReader.Close();

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                    AttributesNode = SubVertex.SelectSingleNode("Attributes");

                    //Adding Vertex Attributes to DB.
                    for (int j = 0; j < Attributes.Length; j++)
                    {

                        XmlNode SingleAttr = AttributesNode.SelectSingleNode(Attributes[j].Name);
                        if (SingleAttr != null)
                        {
                            try
                            {
                                SqlDataReader myReader = null;
                                SqlCommand myCommand = new SqlCommand("INSERT INTO AttrUsage VALUES (newid(), '" + VertexUid + "', '" + Attributes[j].AttributeUID + "', '" + SingleAttr.InnerText + "')", this.MyConnection);
                                myReader = myCommand.ExecuteReader();

                                myReader.Close();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }

                        }
                    }

                    this.SaveToDB(VertexUid, SubVertex, depth + 1);
                }


            }
        }
    }
}