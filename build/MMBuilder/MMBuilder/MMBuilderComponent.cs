using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MMBuilder
{
    public class MMBuilderComponent : GH_Component
    {
        /// <summary>
        /// globals
        /// </summary>
        private List<string> collectionList;
        private List<string> journeyList;
        private Dictionary<string, List<string>> collectionDict;
        private Dictionary<string, string> journeyDict;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MMBuilderComponent()
          : base("MMBuilder", "MMB",
              "temporary description",
              "Circulation Analysis", "Mass Motion")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("run", "R", "Export geometry to Mass Motion. If set to True, this component will auto-update", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("didRun", "O", "new file directory", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            /*
            List<Rhino.Geometry.Mesh> floorList = floors.AllData();
            List<Rhino.Geometry.Mesh> barrierList = barriers.AllData();
            List<Rhino.Geometry.Polyline> pathList = paths.AllData();
            List<Rhino.Geometry.Mesh> linkList = links.AllData();
            List<Rhino.Geometry.Mesh> portalList = portals.AllData();
            List<Rhino.Geometry.Mesh> geometryList = geometry.AllData();
            collectionList = collections.AllData();
            journeyList = journeys.AllData();
            collectionDict = new Dictionary<string, List<string>>();
            journeyDict = new Dictionary<string, string>();
            */



            if (run && fileName != null)
            {
                string pathString = System.IO.Path.Combine(filePath, "gh_out");
                System.IO.Directory.CreateDirectory(pathString);
                string path = pathString + "//" + fileName + ".mmxml";

                using (FileStream filestream = new FileStream(path, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(filestream))
                using (XmlTextWriter writer = new XmlTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;

                    writer.WriteStartDocument();
                    writer.WriteComment("Script written by <Luis Jaggy>");
                    writer.WriteStartElement("DataRoot");
                    writer.WriteAttributeString("FormatVersion", "1");
                    writer.WriteAttributeString("ContentVersion", "8.0.9.0");
                    writer.WriteAttributeString("FileName", "");

                    writer.WriteStartElement("Objects");

                    for (int i = 0; i < portalList.Count; i++)
                    {
                        MMPortal mmPortal = new MMPortal(portalList[i], writer, i.ToString());
                        UpdateJourneys(portalList[i], mmPortal.GlobalID);
                        mmPortal.Write();
                    }
                    for (int i = 0; i < linkList.Count; i++)
                    {
                        MMLink mmLink = new MMLink(linkList[i], writer, i.ToString());
                        UpdateCollections(linkList[i], mmLink.GlobalID);
                        mmLink.Write();
                    }
                    for (int i = 0; i < pathList.Count; i++)
                    {
                        MMPath mmPath = new MMPath(pathList[i], writer, i.ToString());
                        mmPath.Write();
                    }
                    for (int i = 0; i < barrierList.Count; i++)
                    {
                        MMBarrier mmBarrier = new MMBarrier(barrierList[i], writer, i.ToString());
                        UpdateCollections(barrierList[i], mmBarrier.GlobalID);
                        mmBarrier.Write();
                    }
                    for (int i = 0; i < floorList.Count; i++)
                    {
                        MMFloor mmFloor = new MMFloor(floorList[i], writer, i.ToString());
                        UpdateCollections(floorList[i], mmFloor.GlobalID);
                        mmFloor.Write();
                    }
                    for (int i = 0; i < geometryList.Count; i++)
                    {
                        MMVisOnly mmVisOnly = new MMVisOnly(geometryList[i], writer, i.ToString());
                        UpdateCollections(geometryList[i], mmVisOnly.GlobalID);
                        mmVisOnly.Write();
                    }
                    foreach (string key in collectionDict.Keys)
                    {
                        MMCollection mmCollection = new MMCollection(collectionDict[key], writer, key);
                        mmCollection.Write();
                    }
                    foreach (string key in journeyDict.Keys)
                    {
                        MMCirculate mmCirculate = new MMCirculate(journeyDict[key], writer, key);
                        mmCirculate.Write();
                    }
                    writer.WriteEndElement();


                    writer.WriteStartElement("Settings");
                    if (simTime == null)
                    {
                        writer.WriteElementString("Attributes", " ");
                    }
                    else {
                        writer.WriteStartElement("Attributes");
                        writer.WriteStartElement("AttrSettingsSimTimeRange");
                        writer.WriteStartElement("Data");
                        writer.WriteStartElement("BoundedEndTime");
                        writer.WriteAttributeString("v", TimeInSeconds(simTime[1]));
                        writer.WriteAttributeString("t", "3");
                        writer.WriteEndElement();
                        writer.WriteStartElement("BoundedStartTime");
                        writer.WriteAttributeString("v", TimeInSeconds(simTime[0]));
                        writer.WriteAttributeString("t", "3");
                        writer.WriteEndElement();
                        writer.WriteStartElement("TimeRangeType");
                        writer.WriteAttributeString("v", "TimeRangeBounded");
                        writer.WriteAttributeString("t", "3");

                        writer.WriteEndElement();

                        writer.WriteEndElement();
                        writer.WriteStartElement("Type");
                        writer.WriteAttributeString("v", "DataTypeTimeRange");
                        writer.WriteAttributeString("t", "3");
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.WriteEndDocument();

                    //Print("File written as " + path);
                    //System.Diagnostics.Process.Start("explorer.exe", pathString);

                }
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{da756f6c-082c-4a6e-ae01-c850f03c129a}"); }
        }

        /// <summary>
        /// Utility methods for main component run
        /// </summary>
        private void UpdateCollections(Rhino.Geometry.GeometryBase geo, string id)
          {
            foreach ( string collection in collectionList)
            {

              string _collection = collection.Split(':')[1].Trim();

              if (geo.GetUserStrings().AllKeys.Contains(_collection)){
                if (!collectionDict.ContainsKey(_collection)) {
                  collectionDict.Add(_collection, new List<string>());
                }
            collectionDict[_collection].Add(id);
        }
            }
          }
          private void UpdateJourneys(Rhino.Geometry.GeometryBase geo, string id)
        {
            foreach (string journey in journeyList)
            {
                if (geo.GetUserStrings().AllKeys.Contains(journey))
                {

                    string _GUID = geo.GetUserString(journey);
                    string _journey = geo.GetUserString(_GUID);

                    if (!journeyDict.ContainsKey(journey))
                    {
                        journeyDict.Add(journey, _journey);
                    }
                    journeyDict[journey] = journeyDict[journey].Replace(_GUID, id);
                }
            }
        }

        private string TimeInSeconds(double num)
        {
            List<string> timeInSeconds = new List<string> { "00", "00", "00" };
            timeInSeconds[0] = NumToTimeString(Math.Floor(num / 60));
            timeInSeconds[1] = NumToTimeString(num % 60);
            return String.Join(":", timeInSeconds.ToArray());
        }

        private string NumToTimeString(double num)
        {
            if (num.ToString().Length < 2)
            {
                return "0" + num.ToString();
            }
            else {
                return num.ToString();
            }
        }
    }
   ///
   /// MassMotionObject SuperClass that handles geometry conversion to Xml
   /// 
    abstract class MMObject
    {
        public XmlTextWriter _Writer;
        public string _GlobalID;
        public string _ID;
        public string _Name;
        public string _ObjectSubType;
        public string _ObjectType = "Actor";

        // constructor method
        public MMObject(XmlTextWriter writer, string tag)
        {
            this._ID = "_temp" + tag;
            this._GlobalID = Guid.NewGuid().ToString();
            this._Writer = writer;
        }

        public string Name { get { return this._Name; } }
        public XmlTextWriter Writer { get { return this._Writer; } }
        public string GlobalID
        {
            get { return this._GlobalID; }
            set { this._GlobalID = value; }
        }
        public string ID { get { return this._ID; } }
        public string ObjectSubType { get { return this._ObjectSubType; } }
        public string ObjectType { get { return this._ObjectType; } }

        // Write this Geometry's attributes to Xml
        public void Write()
        {
            this.Writer.WriteStartElement(this.Name);

            WriteAttributes();
            WriteBody();
            WriteProperty("GlobalID", this.GlobalID, 3);
            WriteProperty("ID", this.ID, 1);
            WriteProperty("Name", this.Name, 3);
            WriteProperty("ObjectSubType", this.ObjectSubType, 3);
            WriteProperty("ObjectType", this.ObjectType, 3);

            this.Writer.WriteEndElement();
        }

        // Write the attributes element of this object
        public virtual void WriteAttributes()
        {
            this.Writer.WriteElementString("Attributes", " ");
        }

        // Write the body element of this object
        // this method can probably be removed
        public virtual void WriteBody()
        {
            this.Writer.WriteStartElement("Body");
            this.Writer.WriteElementString("Attributes", " ");
            this.Writer.WriteElementString("Geometry", " ");
            this.Writer.WriteEndElement();
        }

        // method to write a generic property to the xml file
        public void WriteProperty(string property, string val, int t)
        {
            this.Writer.WriteStartElement(property);
            this.Writer.WriteAttributeString("v", val);
            this.Writer.WriteAttributeString("t", t.ToString());
            this.Writer.WriteEndElement();
        }

        // method to write an object attribute to the xml file
        public void WriteAttribute(string attr, string data, int datat, string type, int typet)
        {
            this.Writer.WriteStartElement(attr);
            WriteProperty("Data", data, datat);
            WriteProperty("Type", type, typet);
            this.Writer.WriteEndElement();
        }

        // method to write the geometry element of a mesh
        public void WriteMeshGeometry(Rhino.Geometry.Mesh msh)
        {
            this.Writer.WriteStartElement("Geometry");
            WriteProperty("Faces", SerializeMeshFaces(msh), 1);
            WriteProperty("GeometryType", "MeshGeometry", 3);
            WriteProperty("Vertices", SerializeMeshVertices(msh), 2);
            this.Writer.WriteEndElement();
        }

        // method to write the geometry element of a polyline
        public void WritePolyLineGeometry(Rhino.Geometry.Polyline pline)
        {
            this.Writer.WriteStartElement("Geometry")
              ;
            this.Writer.WriteStartElement("CurveData");
            this.WriteProperty("RawPoints", SerializePolylineVertices(pline), 2);
            this.Writer.WriteEndElement();

            this.WriteProperty("EdgeRadius", "0.050000", 2);
            this.WriteProperty("GeometryType", "PolylineGeometry", 3);
            this.WriteProperty("VertexRadius", "0.100000", 2);
            this.Writer.WriteEndElement();
        }

        // Returns a string of serialized mesh faces (flattened)
        public string SerializeMeshFaces(Rhino.Geometry.Mesh msh)
        {
            string serialized = "[]";
            List<string> faces = new List<string>();

            msh.Vertices.CombineIdentical(false, false);

            foreach (Rhino.Geometry.MeshFace face in msh.Faces)
            {
                faces.Add(face.A.ToString());
                faces.Add(face.B.ToString());
                faces.Add(face.C.ToString());
                if (face.IsQuad)
                {
                    faces.Add(face.A.ToString());
                    faces.Add(face.C.ToString());
                    faces.Add(face.D.ToString());
                }
            }
            return serialized.Insert(1, String.Join(",", faces.ToArray()));
        }

        // Returns a string of serialized mesh vertices (flattened)
        public string SerializeMeshVertices(Rhino.Geometry.Mesh msh)
        {
            string serialized = "[]";
            List<string> coords = new List<string>();

            foreach (Rhino.Geometry.Point3d pt in msh.Vertices)
            {
                coords.Add(pt.X.ToString());
                coords.Add(pt.Z.ToString());
                coords.Add((-pt.Y).ToString());
            }
            return serialized.Insert(1, String.Join(",", coords.ToArray()));
        }

        // Returns a string of serialized polyline vertices (flattened)
        public string SerializePolylineVertices(Rhino.Geometry.Polyline pline)
        {
            string serialized = "[]";
            List<string> coords = new List<string>();

            foreach (Rhino.Geometry.Point3d pt in pline)
            {
                coords.Add(pt.X.ToString());
                coords.Add(pt.Z.ToString());
                coords.Add((-pt.Y).ToString());
            }
            return serialized.Insert(1, String.Join(",", coords.ToArray()));
        }

        // Returns a string of serialized list items
        public string SerializeList<T>(List<T> lst)
        {
            string serialized = "[]";
            List<string> serializedList = new List<string>();

            foreach (var item in lst)
            {
                serializedList.Add(item.ToString());
            }
            return serialized.Insert(1, String.Join(",", serializedList.ToArray()));
        }

        // ## not working
        public string SerializeMeshEdges(Rhino.Geometry.Mesh msh)
        {
            string serialized = "[]";
            return serialized;
        }

        // Returns a joined mesh from a list of meshes
        public Mesh JoinMeshes(List<Mesh> meshes)
        {
            var msh = new Rhino.Geometry.Mesh();
            foreach (Rhino.Geometry.Mesh m in meshes)
            {
                msh.Append(m);
            }
            return msh;
        }
    }


    /*
     * MassMotionCollection Class
     */
    class MMCollection : MMObject
    {

        List<string> _IDs;

        public MMCollection(List<string> ids, XmlTextWriter writer, string tag)
          : base(writer, tag)
        {
            this._ObjectSubType = "GroupCollection";
            this._ObjectType = "Group";
            this._Name = tag;
            this._IDs = ids;
        }

        public List<string> IDs { get { return this._IDs; } }

        public override void WriteAttributes()
        {

            List<float> averages = new List<float>();
            int count = this.IDs.Count;
            for (int i = 0; i < count; i++)
            {
                float average = (1F / count);
                averages.Add(average);
            }

            this.Writer.WriteStartElement("Attributes");
            this.Writer.WriteStartElement("AttrCollectionBaseMembers");
            this.Writer.WriteStartElement("Data");
            this.WriteProperty("GlobalIDs", this.SerializeList(this.IDs), 3);
            this.WriteProperty("WeightedType", "WeightedNone", 3);
            this.WriteProperty("Weights", this.SerializeList(averages), 2);
            this.Writer.WriteEndElement();
            this.WriteProperty("Type", "DataTypeVectorWeightedGlobalID", 3);
            this.Writer.WriteEndElement();
            this.Writer.WriteEndElement();
        }

        public override void WriteBody()
        {
        }
    }

    /*
     * MassMotion Circulation and Journey Class
     */
    class MMCirculate : MMObject
    {
        private string _attr;

        public MMCirculate(string attr, XmlTextWriter writer, string tag)
          : base(writer, tag)
        {
            this._ObjectSubType = tag.Split(':')[0] == "Journey" ? "EventJourney" : "EventCirculate";
            //this._ObjectSubType = "EventCirculate";
            this._ObjectType = "Event";
            this._Name = tag.Replace(" ", "");
            this._attr = attr;
        }
        public string Attributes { get { return this._attr; } }

        public override void WriteAttributes()
        {
            this.Writer.WriteRaw(this.Attributes);
        }

        public override void WriteBody()
        {
        }
    }


    /*
     * MassMotionFloor Class
     */
    class MMFloor : MMObject
    {
        Rhino.Geometry.Mesh _msh;

        //constructor method
        public MMFloor(Rhino.Geometry.Mesh msh, XmlTextWriter writer, string tag)
          : base(writer, tag)
        {
            this._msh = msh;
            this._ObjectSubType = "FloorActor";
            this._Name = this.Mesh.ToString().Replace(".", "_") + this.ObjectSubType + tag;
        }
        // return the mesh geometry of this object
        public Rhino.Geometry.Mesh Mesh { get { return this._msh; } }

        // Write the body element of this object
        public override void WriteBody()
        {
            this.Writer.WriteStartElement("Body");
            this.WriteMeshGeometry(this.Mesh);
            this.Writer.WriteEndElement();
        }
    }

    /*
     * MassMotionBarrier Class
     */
    class MMBarrier : MMObject
    {
        Rhino.Geometry.Mesh _msh;

        //constructor method
        public MMBarrier(Rhino.Geometry.Mesh msh, XmlTextWriter writer, string tag)
          : base(writer, tag)
        {
            this._msh = msh;
            this._ObjectSubType = "BarrierActor";
            this._Name = this.Mesh.ToString().Replace(".", "_") + this.ObjectSubType + tag;
        }
        // return the mesh geometry of this object
        public Rhino.Geometry.Mesh Mesh { get { return this._msh; } }

        // Write the body element of this object
        public override void WriteBody()
        {
            this.Writer.WriteStartElement("Body");
            this.WriteMeshGeometry(this.Mesh);
            this.Writer.WriteEndElement();
        }
    }

    /*
     * MassMotionLink Class
     */
    class MMLink : MMObject
    {
        Rhino.Geometry.Mesh _msh;

        // contructor method
        public MMLink(Rhino.Geometry.Mesh msh, XmlTextWriter writer, string tag)
          : base(writer, tag)
        {
            this._msh = msh;
            this._msh.Translate(new Vector3d(0, 0, 0.05));
            this._ObjectSubType = "LinkActor";
            this._Name = this.Mesh.ToString().Replace(".", "_") + this.ObjectSubType + tag;
        }
        // return the mesh geometry of this object
        public Rhino.Geometry.Mesh Mesh { get { return this._msh; } }

        // Write the body element of this object
        public override void WriteBody()
        {

            this.Writer.WriteStartElement("Body");

            this.Writer.WriteStartElement("Attributes");
            this.WriteAttribute("AttrCEActorGoalPointPositions",
              this.SerializeMeshVertices(this.Mesh), 2,
              "DataTypeVectorDouble", 3);
            this.WriteAttribute("AttrCEActorGoalVertexIndices",
              "[1,3,2,0]", 1,
              "DataTypeVectorInt", 3);
            this.Writer.WriteEndElement();
            this.WriteMeshGeometry(this.Mesh);
            this.Writer.WriteEndElement();
        }
    }
    /*
     * MassMotionPortal Class
     */
    class MMPortal : MMObject
    {
        Rhino.Geometry.Mesh _msh;

        // contructor method
        public MMPortal(Rhino.Geometry.Mesh msh, XmlTextWriter writer, string tag)
          : base(writer, tag)
        {
            this._msh = msh;
            this._msh.Translate(new Vector3d(0, 0, 0.05));
            this._ObjectSubType = "PortalActor";
            this._Name = this.Mesh.ToString().Replace(".", "_") + this.ObjectSubType + tag;
        }
        // return the mesh geometry of this object
        public Rhino.Geometry.Mesh Mesh { get { return this._msh; } }

        // Write the body element of this object
        // This currently is not outputting the correct locations for:
        //  - AttrPortalGoalEdgeIndices and
        //  - AttrPortalGoalPointPositions
        public override void WriteBody()
        {
            this.Writer.WriteStartElement("Body");

            this.Writer.WriteStartElement("Attributes");
            this.WriteAttribute("AttrPortalGoalEdgeIndices",
              "[1,4]", 1,
              "DataTypeVectorInt", 3);
            this.WriteAttribute("AttrPortalGoalPointPositions",
              "temp", 2,
              "DataTypeVectorDouble", 3);
            this.Writer.WriteEndElement();
            this.WriteMeshGeometry(this.Mesh);
            this.Writer.WriteEndElement();
        }
    }

    /*
     * MassMotionPath Class
     */
    class MMPath : MMObject
    {
        Rhino.Geometry.Polyline _pline;

        public MMPath(Rhino.Geometry.Polyline pline, XmlTextWriter writer, string tag)
          : base(writer, tag)
        {
            this._pline = pline;
            this._ObjectSubType = "PathActor";
            this._Name = this.Pline.ToString().Replace(".", "_") + this.ObjectSubType + tag;
        }
        // return the polyline geometry of this object
        public Rhino.Geometry.Polyline Pline { get { return this._pline; } }

        // Write the body element of this object
        public override void WriteBody()
        {
            this.Writer.WriteStartElement("Body");
            this.WritePolyLineGeometry(this.Pline);
            this.Writer.WriteEndElement();
        }
    }

    /*
     * MassMotion VisualizationOnly Class
     */
    class MMVisOnly : MMObject
    {
        Rhino.Geometry.Mesh _msh;

        //constructor method
        public MMVisOnly(Rhino.Geometry.Mesh msh, XmlTextWriter writer, string tag)
          : base(writer, tag)
        {
            this._msh = msh;
            this._ObjectSubType = "VisOnlyActor";
            this._Name = this.Mesh.ToString().Replace(".", "_") + this.ObjectSubType + tag;
        }
        // return the mesh geometry of this object
        public Rhino.Geometry.Mesh Mesh { get { return this._msh; } }

        // Write the body element of this object
        public override void WriteBody()
        {
            this.Writer.WriteStartElement("Body");
            this.WriteMeshGeometry(this.Mesh);
            this.Writer.WriteEndElement();
        }
    }
}
