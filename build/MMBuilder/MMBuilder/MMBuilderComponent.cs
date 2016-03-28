using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.IO;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;

namespace MMBuilder
{
    public class MMExport : GH_Component
    {
        /// <summary>
        /// globals
        /// </summary>
        private List<string> collectionList = new List<string>();
        private List<string> journeyList  = new List<string>();
        private Dictionary<string, List<string>> collectionDict;
        private Dictionary<string, string> journeyDict;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MMExport()
          : base("MMExport", "MMExport",
              "temporary description",
              "Circulation Analysis", "Mass Motion")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {

            pManager.AddMeshParameter("Floors", "Floors", "Floor Geometry to export", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Barriers", "Barriers", "Barrier Geometry to export", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Links", "Links", "Link geometry to export", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Portals", "Portals", "Portal geometry to export", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Geometry", "Geometry", " Reference geometry to export", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Paths", "Paths", "Path geometry to export", GH_ParamAccess.tree);

            pManager.AddTextParameter("Collections", "Collections", "MMCollection objects to export", GH_ParamAccess.tree);
            pManager.AddTextParameter("Journeys", "Journeys", "MMJourney objects to export", GH_ParamAccess.tree);


            pManager.AddTextParameter("FilePath", "FilePath", "Directory for .mmxml export", GH_ParamAccess.item);
            pManager.AddTextParameter("FileName", "FileName", "Name of file to export", GH_ParamAccess.item);

            pManager.AddBooleanParameter("Run",
                "Run",
                "Export toggle. If set to True, the new file will auto-update with any geometry changes.",
                GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("File", "File", "new file directory", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            string simTime = null; // simtime to be implemented
            string fileName = null;
            string filePath = null;
            bool run = false;

            GH_Structure<GH_Mesh> FloorTree= new GH_Structure<GH_Mesh>();
            GH_Structure<GH_Mesh> BarrierTree = new GH_Structure<GH_Mesh>();
            GH_Structure<GH_Mesh> LinkTree = new GH_Structure<GH_Mesh>();
            GH_Structure<GH_Mesh> PortalTree = new GH_Structure<GH_Mesh>();
            GH_Structure<GH_Mesh> GeomTree = new GH_Structure<GH_Mesh>();
            GH_Structure<GH_Curve> PathTree = new GH_Structure<GH_Curve>();
            GH_Structure<GH_String> CollectionTree = new GH_Structure<GH_String>();
            GH_Structure<GH_String> JourneyTree = new GH_Structure<GH_String>();

            DA.GetDataTree("Floors", out FloorTree);
            DA.GetDataTree("Barriers", out BarrierTree);
            DA.GetDataTree("Links", out LinkTree);
            DA.GetDataTree("Portals", out PortalTree);
            DA.GetDataTree("Geometry", out GeomTree);
            DA.GetDataTree("Paths", out PathTree);
            DA.GetDataTree("Journeys", out JourneyTree);
            DA.GetDataTree("Collections", out CollectionTree);

            List<Mesh> floorList = new List<Mesh>();
            List<Mesh> barrierList = new List<Mesh>();
            List<Mesh> linkList = new List<Mesh>();
            List<Mesh> portalList = new List<Mesh>();
            List<Mesh> geometryList = new List<Mesh>();
            List<Polyline> pathList = new List<Polyline>();

            foreach (GH_Mesh msh in FloorTree.AllData(true))
            {
                floorList.Add(msh.Value);
            }
            foreach (GH_Mesh msh in BarrierTree.AllData(true))
            {
                barrierList.Add(msh.Value);
            }
            foreach (GH_Mesh msh in LinkTree.AllData(true))
            {
                linkList.Add(msh.Value);
            }
            foreach (GH_Mesh msh in PortalTree.AllData(true))
            {
                portalList.Add(msh.Value);
            }
            foreach (GH_Mesh msh in GeomTree.AllData(true))
            {
                geometryList.Add(msh.Value);
            }
            foreach (GH_Curve crv in PathTree.AllData(true))
            {
                Polyline pline;
                if(crv.Value.TryGetPolyline(out pline))
                {
                    pathList.Add(pline);
                }

            }
            foreach (GH_String str in CollectionTree.AllData(true))
            {
                if (str.Value != null)
                {
                    collectionList.Add(str.Value);
                } 
                
            }
            foreach (GH_String str in JourneyTree.AllData(true))
            {
                if (str.Value != null)
                {
                    journeyList.Add(str.Value);
                }
            }


            collectionDict = new Dictionary<string, List<string>>();
            journeyDict = new Dictionary<string, string>();

            if (!DA.GetData(8, ref filePath)) return;
            if (!DA.GetData(9, ref fileName)) return;
            if (!DA.GetData(10, ref run)) return;
            if (run && (fileName != null))
            {
                string pathString = System.IO.Path.Combine(filePath, "gh_out");
                Directory.CreateDirectory(pathString);
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

                    string res = "File written as " + path;
                    //System.Diagnostics.Process.Start("explorer.exe", pathString);
                    DA.SetData("File", res);

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
        private void UpdateCollections(GeometryBase geo, string id)
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
          private void UpdateJourneys(GeometryBase geo, string id)
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

    public class MMJourney : GH_Component
    {

        public MMJourney()
            : base("MMJourney", "MMJourney", 
                  "temp decription",
                  "Circulation Analysis", "Mass Motion")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("StartPortals", "StartPortals", "temp desciption", GH_ParamAccess.tree);
            pManager.AddNumberParameter("StartWeights", "StartWeights", "temp description", GH_ParamAccess.tree);
            pManager.AddMeshParameter("EndPortals", "EndPortals", "temp desciption", GH_ParamAccess.tree);
            pManager.AddNumberParameter("EndWeights", "EndWeights", "temp description", GH_ParamAccess.tree);
            pManager.AddMeshParameter("DwellPortals", "DwellPortals", "temp desciption", GH_ParamAccess.tree);
            pManager.AddNumberParameter("DwelltWeights", "DwellWeights", "temp description", GH_ParamAccess.tree);

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("out", "O", "temp", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            if (startPortals.BranchCount != 0 &&
              endPortals.BranchCount != 0 &&
              agentNum != null &&
              simTime != null &&
              name != null)
            {

                string journeyName = dwellPortals.AllData().Count != 0 ? "Circulate: " + name : "Journey: " + name;
                string startWeightsString;
                string endWeightsString;
                string dwellWeightsString;

                List<string> startPortalIDs = new List<string>();
                List<string> endPortalIDs = new List<string>();
                List<string> dwellPortalIDs = new List<string>();

                foreach (Mesh portal in startPortals.AllData())
                {
                    var id = Guid.NewGuid().ToString();
                    portal.SetUserString(journeyName, id);
                    startPortalIDs.Add(id);
                }
                foreach (Mesh portal in endPortals.AllData())
                {
                    var id = Guid.NewGuid().ToString();
                    portal.SetUserString(journeyName, id);
                    endPortalIDs.Add(id);
                }
                foreach (Mesh portal in dwellPortals.AllData())
                {
                    var id = Guid.NewGuid().ToString();
                    portal.SetUserString(journeyName, id);
                    dwellPortalIDs.Add(id);
                }

                if (endWeights.AllData().Count() == endPortals.AllData().Count())
                {
                    endWeightsString = SerializeList(endWeights.AllData());
                }
                else if (endWeights.BranchCount == 0)
                {
                    endWeightsString = SerializeList(AverageNumList(endPortalIDs.Count()));
                }
                else {
                    Print("WARNING: end weight count not equal to end portal count: using uniform distribution");
                    endWeightsString = SerializeList(AverageNumList(endPortalIDs.Count()));
                }
                if (startWeights.AllData().Count() == startPortals.AllData().Count())
                {
                    startWeightsString = SerializeList(startWeights.AllData());
                }
                else if (startWeights.BranchCount == 0)
                {
                    startWeightsString = SerializeList(AverageNumList(startPortalIDs.Count()));
                }
                else {
                    Print("WARNING: start weight count not equal to start portal count: using uniform distribution");
                    startWeightsString = SerializeList(AverageNumList(startPortalIDs.Count()));
                }

                Attribute _temp = new Attribute("Attributes");

                if (dwellPortalIDs.Count != 0)
                {

                    if (!simTime.IncludesInterval(dwellTime))
                    {
                        Print("WARNING: dwellTime not within simTime: running dwellTime for entire Simulation");
                        dwellTime = simTime;
                    }
                    if (dwellWeights.AllData().Count() == dwellPortals.AllData().Count())
                    {
                        dwellWeightsString = SerializeList(dwellWeights.AllData());
                    }
                    else if (dwellWeights.BranchCount == 0)
                    {
                        dwellWeightsString = SerializeList(AverageNumList(dwellPortalIDs.Count()));
                    }
                    else {
                        Print("WARNING: dwell weight count not equal to dwell portal count: using uniform distribution");
                        dwellWeightsString = SerializeList(AverageNumList(dwellPortalIDs.Count()));
                    }

                    _temp.AddDataTypeVectorWeightedGlobalIDAttribute("AttrCirculateEventCirculatePortals",
                      SerializeList(dwellPortalIDs),
                      dwellWeightsString); // weights
                    _temp.AddDataTypeBoolAttribute("AttrCirculateEventCirculateWaitOnStart", "0");
                    _temp.AddDataTypeEnumAttribute("AttrCirculateEventCirculateWaitStyle", "WaitSpreadOut", "2");
                    _temp.AddDataTypeDistributionAttribute("AttrCirculateEventLifetimeCountDistribution",
                      "Uniform",
                      "[0.000000,10.000000]"); // work this out later
                    _temp.AddDataTypeDistributionAttribute("AttrCirculateEventLifetimeDurationDistribution",
                      "Uniform",
                      SerializeList(new List<double> { dwellTime[0] * 60F, dwellTime[1] * 60F }));
                    _temp.AddDataTypeDataTypeTimeReferenceAttribute("AttrEventCirculateLifetimeEndTime",
                      "00000000-0000-0000-0000-000000000000",
                      TimeInSeconds(dwellTime[1]));
                    _temp.AddDataTypeEnumAttribute("AttrCirculateEventLifetimeType", "LifetimeForever", "0");
                    _temp.AddDataTypeBoolAttribute("AttrCirculateEventLifetimeWaitAfterCount", "0");


                }
                _temp.AddDataTypeBoolAttribute("AttrEnabled", "1");
                _temp.AddDataTypeActionAttribute("AttrEventBirthAction", "ActionNone");
                _temp.AddDataTypeAttribute("AttrEventBirthProfile", "00000000-0000-0000-0000-000000000000", "DataTypeGlobalID");
                _temp.AddDataTypeAttribute("AttrEventDemandCurveData", "[]", "DataTypeVectorDouble");
                _temp.AddDataTypeEnumAttribute("AttrEventDemandType", "DemandDistribution", "2");
                _temp.AddDataTypeEnumAttribute("AttrEventDestinationType", "DestinationAssigned", "1");
                _temp.AddDataTypeDistributionAttribute("AttrEventDurationDistribution",
                  "Uniform",
                  SerializeList(new List<double> { simTime[0] * 60F, simTime[1] * 60F }));
                _temp.AddDataTypeVectorWeightedGlobalIDAttribute("AttrEventMultiDestination",
                  SerializeList(endPortalIDs),
                  endWeightsString); // weights
                _temp.AddDataTypeVectorWeightedGlobalIDAttribute("AttrEventMultiOrigin",
                  SerializeList(startPortalIDs),
                  startWeightsString);
                _temp.AddDataTypeAttribute("AttrEventPopulation", agentNum, "DataTypeInt");
                _temp.AddDataTypeDataTypeTimeReferenceAttribute("AttrEventStartTime",
                  "00000000-0000-0000-0000-000000000000",
                  TimeInSeconds(simTime[0]));

                foreach (Mesh portal in startPortals.AllData())
                {
                    portal.SetUserString(portal.GetUserString(journeyName), _temp.ToString());
                }
                foreach (Mesh portal in endPortals.AllData())
                {
                    portal.SetUserString(portal.GetUserString(journeyName), _temp.ToString());
                }
                foreach (Mesh portal in startPortals.AllData())
                {
                    portal.SetUserString(portal.GetUserString(journeyName), _temp.ToString());
                }

                A = journeyName;
            }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get
            {
                return Guid.NewGuid();
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

        private string SerializeList<T>(List<T> lst)
        {
            string serialized = "[]";
            List<string> serializedList = new List<string>();

            foreach (var item in lst)
            {
                serializedList.Add(item.ToString());
            }
            return serialized.Insert(1, String.Join(",", serializedList.ToArray()));
        }

        private List<float> AverageNumList(double num)
        {
            List<float> averages = new List<float>();
            float average = (1F / (float)num);
            for (int i = 0; i < num; i++) averages.Add(average);
            return averages;
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
        public void WriteMeshGeometry(Mesh msh)
        {
            this.Writer.WriteStartElement("Geometry");
            WriteProperty("Faces", SerializeMeshFaces(msh), 1);
            WriteProperty("GeometryType", "MeshGeometry", 3);
            WriteProperty("Vertices", SerializeMeshVertices(msh), 2);
            this.Writer.WriteEndElement();
        }

        // method to write the geometry element of a polyline
        public void WritePolyLineGeometry(Polyline pline)
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
        public string SerializeMeshFaces(Mesh msh)
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
        public string SerializeMeshVertices(Mesh msh)
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
        public string SerializePolylineVertices(Polyline pline)
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
        public string SerializeMeshEdges(Mesh msh)
        {
            string serialized = "[]";
            return serialized;
        }

        // Returns a joined mesh from a list of meshes
        public Mesh JoinMeshes(List<Mesh> meshes)
        {
            var msh = new Mesh();
            foreach (Mesh m in meshes)
            {
                msh.Append(m);
            }
            return msh;
        }
    }

    /// <summary>
    /// Mass Motion Collection class
    /// </summary>
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

    /// <summary>
    /// Mass Motion Circulate class
    /// </summary>
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

    /// <summary>
    /// Mass Motion Floor class
    /// </summary>
    class MMFloor : MMObject
    {
        Mesh _msh;

        //constructor method
        public MMFloor(Mesh msh, XmlTextWriter writer, string tag)
          : base(writer, tag)
        {
            this._msh = msh;
            this._ObjectSubType = "FloorActor";
            this._Name = this.Mesh.ToString().Replace(".", "_") + this.ObjectSubType + tag;
        }
        // return the mesh geometry of this object
        public Mesh Mesh { get { return this._msh; } }

        // Write the body element of this object
        public override void WriteBody()
        {
            this.Writer.WriteStartElement("Body");
            this.WriteMeshGeometry(this.Mesh);
            this.Writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Mass Motion Barrier class
    /// </summary>
    class MMBarrier : MMObject
    {
        Mesh _msh;

        //constructor method
        public MMBarrier(Mesh msh, XmlTextWriter writer, string tag)
          : base(writer, tag)
        {
            this._msh = msh;
            this._ObjectSubType = "BarrierActor";
            this._Name = this.Mesh.ToString().Replace(".", "_") + this.ObjectSubType + tag;
        }
        // return the mesh geometry of this object
        public Mesh Mesh { get { return this._msh; } }

        // Write the body element of this object
        public override void WriteBody()
        {
            this.Writer.WriteStartElement("Body");
            this.WriteMeshGeometry(this.Mesh);
            this.Writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Mass Motion Link class
    /// </summary>
    class MMLink : MMObject
    {
        Mesh _msh;

        // contructor method
        public MMLink(Mesh msh, XmlTextWriter writer, string tag)
          : base(writer, tag)
        {
            this._msh = msh;
            this._msh.Translate(new Vector3d(0, 0, 0.05));
            this._ObjectSubType = "LinkActor";
            this._Name = this.Mesh.ToString().Replace(".", "_") + this.ObjectSubType + tag;
        }
        // return the mesh geometry of this object
        public Mesh Mesh { get { return this._msh; } }

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

    /// <summary>
    /// Mass Motion Portal class
    /// </summary>
    class MMPortal : MMObject
    {
        Mesh _msh;

        // contructor method
        public MMPortal(Mesh msh, XmlTextWriter writer, string tag)
          : base(writer, tag)
        {
            this._msh = msh;
            this._msh.Translate(new Vector3d(0, 0, 0.05));
            this._ObjectSubType = "PortalActor";
            this._Name = this.Mesh.ToString().Replace(".", "_") + this.ObjectSubType + tag;
        }
        // return the mesh geometry of this object
        public Mesh Mesh { get { return this._msh; } }

        // Write the body element of this object
        // This currently is not outputting the correct locations for:
        //  - AttrPortalGoalEdgeIndices and
        //  - AttrPortalGoalPointPositions
        public override void WriteBody()
        {
            this.Writer.WriteStartElement("Body");
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

    /// <summary>
    /// Mass Motion Path class
    /// </summary>
    class MMPath : MMObject
    {
        Polyline _pline;

        public MMPath(Polyline pline, XmlTextWriter writer, string tag)
          : base(writer, tag)
        {
            this._pline = pline;
            this._ObjectSubType = "PathActor";
            this._Name = this.Pline.ToString().Replace(".", "_") + this.ObjectSubType + tag;
        }
        // return the polyline geometry of this object
        public Polyline Pline { get { return this._pline; } }

        // Write the body element of this object
        public override void WriteBody()
        {
            this.Writer.WriteStartElement("Body");
            this.WritePolyLineGeometry(this.Pline);
            this.Writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Mass Motion Visualisation Only class
    /// </summary>
    class MMVisOnly : MMObject
    {
        Mesh _msh;

        //constructor method
        public MMVisOnly(Mesh msh, XmlTextWriter writer, string tag)
          : base(writer, tag)
        {
            this._msh = msh;
            this._ObjectSubType = "VisOnlyActor";
            this._Name = this.Mesh.ToString().Replace(".", "_") + this.ObjectSubType + tag;
        }
        // return the mesh geometry of this object
        public Mesh Mesh { get { return this._msh; } }

        // Write the body element of this object
        public override void WriteBody()
        {
            this.Writer.WriteStartElement("Body");
            this.WriteMeshGeometry(this.Mesh);
            this.Writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Attribute class to write geometry to an Mass Motion Xml Attribute
    /// </summary>
    class Attribute
    {
        private string _name;
        private string _v;
        private string _t;
        private Dictionary<string, Attribute> _attr = new Dictionary<string, Attribute>();

        public Attribute(string name)
        {
            this._name = name;
        }
        public string Name { get { return this._name; } }
        public string V
        {
            get { return this._v; }
            set { this._v = value; }
        }
        public string T
        {
            get { return this._t; }
            set { this._t = value; }
        }
        public Dictionary<string, Attribute> Attributes { get { return this._attr; } }


        public void AddAttribute(string name, string v, string t)
        {
            var attr = new Attribute(name);
            this._attr.Add(name, attr);
            if (v != "") { attr.V = v; }
            if (t != "") { attr.T = t; }
        }

        public void AddNestedAttribute(string name, List<List<string>> subAttr)
        {
            this.AddAttribute(name, "", "");
            foreach (List<string> attr in subAttr)
            {
                this.Attributes[name].AddAttribute(attr[0], attr[1], attr[2]);
            }
        }

        public override string ToString()
        {
            string parsed = "<" + this.Name.ToString();

            if (this.V != null)
            {
                parsed += " v=" + "\"" + this.V + "\" ";
            }
            if (this.T != null)
            {
                parsed += " t=" + "\"" + this.T + "\" ";
            }
            parsed += ">";

            foreach (string key in this.Attributes.Keys)
            {
                parsed += this.Attributes[key].ToString();
            }

            return parsed + "</" + this.Name.ToString() + ">";
        }

        /*
         * Mass Motion specific XML types
         */
        public void AddDataTypeAttribute(string name, string data, string type)
        {
            this.AddNestedAttribute(name, new List<List<string>> {
          new List < string > {"Data", data, "1"},
          new List < string > {"Type", type, "3"}
          });
        }

        public void AddDataTypeBoolAttribute(string name, string data)
        {
            this.AddNestedAttribute(name, new List<List<string>> {
          new List < string > {"Data", data, "0"},
          new List < string > {"Type", "DataTypeBool", "3"}
          });
        }

        public void AddDataTypeEnumAttribute(string name, string enumString, string enumValue)
        {
            this.AddAttribute(name, "", "");
            this.Attributes[name].AddNestedAttribute("Data", new List<List<string>> {
          new List < string > {"EnumString", enumString, "3"},
          new List < string > {"EnumValue", enumValue, "1"}
          });
            this.Attributes[name].AddAttribute("Type", "DataTypeEnum", "3");
        }

        public void AddDataTypeVectorWeightedGlobalIDAttribute(string name, string ids, string weights)
        {
            this.AddAttribute(name, "", "");
            this.Attributes[name].AddNestedAttribute("Data", new List<List<string>> {
          new List < string > {"GlobalIDs", ids, "3"}, // list of GUIDs
          new List < string > {"WeightedType", "WeightedNone", "3"},
          new List < string > {"Weights", weights, "2"} // weighted average
          });
            this.Attributes[name].AddAttribute("Type", "DataTypeVectorWeightedGlobalID", "3");
        }

        public void AddDataTypeDataTypeTimeReferenceAttribute(string name, string guid, string time)
        {
            this.AddAttribute(name, "", ""); // cannot add optional parameters
            this.Attributes[name].AddNestedAttribute("Data", new List<List<string>> {
          new List < string > {"GlobalID", guid, "3"},
          new List < string > {"TimeInSeconds", time, "3"},
          new List < string > {"TimeType", "TimeSimulationStart", "3"}
          });
            this.Attributes[name].AddAttribute("Type", "DataTypeTimeReference", "3");
        }

        public void AddDataTypeDistributionAttribute(string name, string type, string values)
        {
            this.AddAttribute(name, "", "");
            this.Attributes[name].AddNestedAttribute("Data", new List<List<string>> {
          new List < string > {"Type", type, "3"},
          new List < string > {"Values", values, "2"} // weighted average
          });
            this.Attributes[name].AddAttribute("Type", "DataTypeDistribution", "3");
        }

        public void AddDataTypeActionAttribute(string name, string action)
        {
            this.AddAttribute(name, "", "");
            this.Attributes[name].AddNestedAttribute("Data", new List<List<string>> {
          new List < string > {"ActionType", action, "3"},
          });
            this.Attributes[name].AddAttribute("Type", "DataTypeAction", "3");
        }
    }
}
