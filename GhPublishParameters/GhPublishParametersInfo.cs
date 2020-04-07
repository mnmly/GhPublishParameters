using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace GhPublishParameters
{
    public class GhPublishParametersInfo : GH_AssemblyInfo
    {
        public override string Name => "GhPublishParameters";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("93C6AA74-2857-4C39-AE0E-D63EF964D42F");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}