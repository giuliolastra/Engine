using System;
using System.Data.SqlClient;

namespace Engine
{
    class Calculus
    {
        SqlConnection MyConnection;

        public Calculus()
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
                Console.WriteLine("Impossibile aprire la connessione: "+e.ToString());
            }
        }

        public String PathCalculus(String type, String vertexA, String vertexB)
        {
            int depthA = 0;
            int depthB = 0;
            String VertexUIDA = "";
            String VertexUIDB = "";


            //Try catch block to see the depth of vertexA.
            try
            {
                SqlDataReader myReader = null;
                SqlCommand myCommand = new SqlCommand("SELECT VertexUid, Depth FROM Vertex WHERE Type = '"+type+"' AND Name = '"+vertexA+"';", this.MyConnection);
                myReader = myCommand.ExecuteReader();

                while (myReader.Read())
                {
                   int.TryParse(myReader["Depth"].ToString(), out depthA);

               //VertexUid string.

                    VertexUIDA = myReader["VertexUid"].ToString();
                    break;
                }

                myCommand = null;
                myReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Errore su Vertex A: "+e.ToString());
            }

            try
            {
                SqlDataReader myReader = null;
                SqlCommand myCommand = new SqlCommand("SELECT VertexUid, Depth FROM Vertex WHERE Type = '" + type + "' AND Name = '" + vertexB + "';", this.MyConnection);
                myReader = myCommand.ExecuteReader();

                while (myReader.Read())
                {
                    int.TryParse(myReader["Depth"].ToString(), out depthB);


                    VertexUIDB = myReader["VertexUid"].ToString();
                    break;
                }

                myCommand = null;
                myReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Errore su Vertex B: "+e.ToString());
            }

            // Initialize vertex UID assigning the summit that is deeper.
            String VertexUID = "";
            String VertexName = "";
                        
            if (depthA > depthB)
            {
                VertexUID = VertexUIDA;
                VertexName = vertexA;
            }
            else
            {
                VertexUID = VertexUIDB;
                VertexName = vertexB;
            }

            String EdgeUID = getEdgeUIDFromVertex(VertexUID);
            // Account attributes to initialize ResultCollector.
            int AttributeNumber = getAttributeSize(VertexUID) + getAttributeSize(EdgeUID);
            
            
            ResultCollector result = new ResultCollector(AttributeNumber+1);

            //Adding first vertex to VertexNameList.
            result.addVertexName(VertexName);


            // Count all attributes.
            int depthDiff = Math.Abs(depthA - depthB);

            for (int i = 0; i < depthDiff; ++i) 
            {

                String AttrName = "";
                int AttrValue = 0;
                
                try
                {
                    SqlDataReader myReader = null;
                    SqlCommand myCommand = new SqlCommand("SELECT AD.AttrDefUid, AD.Name, AU.AttrValue FROM AttrDef AD, AttrUsage AU WHERE AU.ObjectUid = '" + VertexUID + "' AND AU.AttrDefUid = AD.AttrDefUid; ", this.MyConnection);
                    myReader = myCommand.ExecuteReader();

                    while (myReader.Read())
                    {
                        int.TryParse(myReader["AttrValue"].ToString(), out AttrValue);

                        AttrName = myReader["Name"].ToString();

                        if (i == 0)
                        {
                            result.addNewAttribute(AttrName, AttrValue);
                        }
                        else
                        {
                            result.addAttribute(AttrName, AttrValue);

                        }
                    }

                    myReader.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                
                EdgeUID = getEdgeUIDFromVertex(VertexUID);

                    try
                    {
                        SqlDataReader myReader = null;
                        SqlCommand myCommand = new SqlCommand("SELECT AD.AttrDefUid, AD.Name, AU.AttrValue FROM AttrDef AD, AttrUsage AU WHERE AU.ObjectUid = '" + EdgeUID + "' AND AU.AttrDefUid = AD.AttrDefUid; ", this.MyConnection);
                        myReader = myCommand.ExecuteReader();

                        while (myReader.Read())
                        {
                            int.TryParse(myReader["AttrValue"].ToString(), out AttrValue);

                            AttrName = myReader["Name"].ToString();

                        if (i == 0)
                        {
                            result.addNewAttribute(AttrName, AttrValue);
                        }
                        else result.addAttribute(AttrName, AttrValue);
                        }

                        myReader.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    

              
                VertexUID = getVertexUIDFromEdge(EdgeUID);
                VertexName = getVertexName(VertexUID);
                result.addVertexName(VertexName);
            }

            return result.toString();
        }

        private String getEdgeUIDFromVertex(String VertexUID)
        {
            String EdgeUID  = "";
            // See the linked edge
            try
            {
                SqlDataReader myReader = null;
                SqlCommand myCommand = new SqlCommand("SELECT * FROM Vertex WHERE VertexUid = '" + VertexUID + "'", this.MyConnection);
                myReader = myCommand.ExecuteReader();

                while (myReader.Read())
                {
                    EdgeUID = myReader["PreviousEdgeUid"].ToString();
                    break;
                }
                
                myReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Errore EdgeUID From Vertex: "+e.ToString());
            }
            return EdgeUID;

        }
        private String getVertexUIDFromEdge(String EdgeUID)
        {
            String VertexUID = "";

            try
            {
                SqlDataReader myReader = null;
                SqlCommand myCommand = new SqlCommand("SELECT * FROM Edge WHERE EdgeUid = '" + EdgeUID + "'", this.MyConnection);
                myReader = myCommand.ExecuteReader();

                while (myReader.Read())
                {
                    VertexUID = myReader["StartVertexUid"].ToString();
                    break;
                }

                myReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return VertexUID;

        }
        private String getVertexName(String VertexUID)
        {
            String VertexName = "";

            try
            {
                SqlDataReader myReader = null;
                SqlCommand myCommand = new SqlCommand("SELECT Name FROM Vertex WHERE VertexUid = convert(uniqueidentifier, '" + VertexUID + "') ", this.MyConnection);
                myReader = myCommand.ExecuteReader();

                while (myReader.Read())
                {
                    VertexName = myReader["Name"].ToString();
                    break;
                }

                myReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return VertexName;

        }
        private int getAttributeSize(String UID)
        {
            int AttributeNumber = 0;

            // See a vertex attribute.
            try
            {
                SqlDataReader myReader = null;
                SqlCommand myCommand = new SqlCommand("SELECT COUNT(distinct AU.AttrValue) AS AttrNumber FROM AttrDef AD, AttrUsage AU WHERE AU.ObjectUid = '" + UID + "' AND AU.AttrDefUid = AD.AttrDefUid; ", this.MyConnection);
                myReader = myCommand.ExecuteReader();

                while (myReader.Read())
                {
                    int.TryParse(myReader["AttrNumber"].ToString(), out AttributeNumber);
                }

                myReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return AttributeNumber;
        }
    }
}
