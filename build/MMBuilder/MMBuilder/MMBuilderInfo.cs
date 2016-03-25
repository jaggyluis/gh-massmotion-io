using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace MMBuilder
{
    public class MMBuilderInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "MMBuilder";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("39455b87-e520-4a58-9a14-3655e4aa1c44");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Woods Bagot";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
