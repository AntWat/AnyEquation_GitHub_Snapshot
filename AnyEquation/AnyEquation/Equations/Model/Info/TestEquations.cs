using AnyEquation.Common;
using AnyEquation.Equations.EquationParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static AnyEquation.Equations.Model.ContentManager;

namespace AnyEquation.Equations.Model.Info
{
    public static class TestEquations
    {

        #region ------------ Test Routines ------------

        public static ContentManager _contentManager { get; private set; }

        public static IList<EquationLibrary> CreateTestEquationLibraries(ContentManager contentManager)
        {
            _contentManager = contentManager;

            IList<EquationLibrary> equationLibraries = new List<EquationLibrary>();

            equationLibraries.Add(CreateFavoritesLibrary());
            //equationLibraries.Add(CreateTestGeickLibrary());
            equationLibraries.Add(CreateTestMiscellaneousLibrary());

            AddEmptySampleLibraries(equationLibraries);


            return equationLibraries;
        }

        private static void AddEmptySampleLibraries(IList<EquationLibrary> equationLibraries)
        {
            equationLibraries.Add(new EquationLibrary()
            {
                Name = "Perry's Chemical Engineers' Handbook",
                Description = "",
            });

            equationLibraries.Add(new EquationLibrary()
            {
                Name = "Rules of Thumb for Maintenance and Reliability Engineers",
                Description = "",
            });

            equationLibraries.Add(new EquationLibrary()
            {
                Name = "Rules of Thumb for Chemical Engineers",
                Description = "",
            });

            equationLibraries.Add(new EquationLibrary()
            {
                Name = "Pipeline Rules of Thumb Handbook: A Manual of Quick, Accurate Solutions to Everyday Pipeline Engineering Problems",
                Description = "",
            });

            equationLibraries.Add(new EquationLibrary()
            {
                Name = "Geotechnical Engineering Calculations and Rules of Thumb",
                Description = "",
            });

            equationLibraries.Add(new EquationLibrary()
            {
                Name = "Rules of Thumb in Engineering Practice",
                Description = "",
            });

            equationLibraries.Add(new EquationLibrary()
            {
                Name = "Engineering Formulas (Speedy Study Guides)",
                Description = "",
            });

            equationLibraries.Add(new EquationLibrary()
            {
                Name = "Engineering Formulas (Quick Study Academic)",
                Description = "",
            });

            equationLibraries.Add(new EquationLibrary()
            {
                Name = "Fire-Safety Engineering: Formula Finder",
                Description = "",
            });

            equationLibraries.Add(new EquationLibrary()
            {
                Name = "Structural Engineering Formulas",
                Description = "",
            });

            equationLibraries.Add(new EquationLibrary()
            {
                Name = "Reeds Mathematical Tables and Engineering Formulae",
                Description = "",
            });

            equationLibraries.Add(new EquationLibrary()
            {
                Name = "Civil Engineering Formulas",
                Description = "",
            });

            equationLibraries.Add(new EquationLibrary()
            {
                Name = "Mechanical Engineering Formulas Pocket Guide (Pocket Reference)",
                Description = "",
            });

            equationLibraries.Add(new EquationLibrary()
            {
                Name = "Perry's Standard Tables and Formulae For Chemical Engineers (Professional Engineering)",
                Description = "",
            });

        }

        private static EquationLibrary CreateFavoritesLibrary()
        {
            IList<EquationDetails> equationDetails = new List<EquationDetails>();

            EquationLibrary equationLibrary = new EquationLibrary()
            {
                Name = "Favorites",
                Description = "",
            };

            SectionNode[] sn = new SectionNode[10];
            SectionNode eqnSn;

            // ---------------------
            sn[0] = new SectionNode(null, "Favorites"); equationLibrary.TopSectionNodes.Add(sn[0]);

            eqnSn = sn[0];
            equationDetails.Clear();

            //Equation b33: Area of an annulus
            equationDetails.Add(new EquationDetails("Area of an annulus", "A = (PI/4)*(D^2-d^2)",
                new List<VarInfo>()
                {
                    new VarInfo("A", "A", _contentManager.ParamTypes["area"]),
                    new VarInfo("D", "D", _contentManager.ParamTypes["length"]),
                    new VarInfo("d", "d", _contentManager.ParamTypes["length"]),
                }));

            //Equation c43: ungula
            equationDetails.Add(new EquationDetails("c43: ungula", "A0 = Am + (PI/2)*r^2 + (PI/2)*r*Sqrt(r^2+h^2)", null));

            equationDetails.Add(new EquationDetails("Heat duty from Area", "Q = U*A*DTLM", null));
            equationDetails.Add(new EquationDetails("Log mean temperature difference", "DTLM = (DT1-DT2)/LOG(DT1/DT2)", null));

            equationDetails.Add(new EquationDetails("Einstein's famous equation", "E = m*c^2",
                new List<VarInfo>()
                {
                    new VarInfo("E", "Energy available from converting mass to pure energy", _contentManager.ParamTypes["energy"]),
                    new VarInfo("m", "mass", _contentManager.ParamTypes["mass"]),
                }));

            EquationLibrary.AddEquationsToSectionNode(_contentManager, eqnSn, equationDetails);
            // ---------------------

            return equationLibrary;
        }


        //private static EquationLibrary CreateTestGeickLibrary()
        //{
        //    IList<EquationDetails> equationDetails = new List<EquationDetails>();

        //    EquationLibrary equationLibrary = new EquationLibrary()
        //    {
        //        Name = "Test: Gieck Engineering Formulas",
        //        Description = "",
        //    };

        //    SectionNode[] sn = new SectionNode[10];
        //    SectionNode eqnSn;

        //    // ---------------------
        //    //sn[0] = new SectionNode(null, "A) Units"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "B) Areas"); equationLibrary.TopSectionNodes.Add(sn[0]);
        //    sn[1] = new SectionNode(sn[0], "Page B1");
        //    sn[2] = new SectionNode(sn[1], "Square");

        //    eqnSn = sn[2];
        //    equationDetails.Clear();

        //    equationDetails.Add(new EquationDetails("Equation: b 1", "A = a^2",
        //        new List<VarInfo>()
        //        {
        //            new VarInfo("A", "A", _contentManager.ParamTypes["area"]),
        //            new VarInfo("a", "a", _contentManager.ParamTypes["length"]),
        //        }));
        //    equationDetails.Add(new EquationDetails("Equation: b 2", "a = Sqrt(A)",
        //        new List<VarInfo>()
        //        {
        //            new VarInfo("A", "A", _contentManager.ParamTypes["area"]),
        //            new VarInfo("a", "a", _contentManager.ParamTypes["length"]),
        //        }));
        //    equationDetails.Add(new EquationDetails("Equation: b 3", "d = a*Sqrt(2)",
        //        new List<VarInfo>()
        //        {
        //            new VarInfo("a", "a", _contentManager.ParamTypes["length"]),
        //            new VarInfo("d", "d", _contentManager.ParamTypes["length"]),
        //        }));

        //    EquationLibrary.AddEquationsToSectionNode(_contentManager, eqnSn, equationDetails);

        //    // ---------------------
        //    sn[1] = new SectionNode(sn[0], "Page B3");
        //    sn[2] = new SectionNode(sn[1], "Annulus");

        //    eqnSn = sn[2];
        //    equationDetails.Clear();

        //    //Equation b33: Area of an annulus
        //    equationDetails.Add(new EquationDetails("Area of an annulus", "A = (PI/4)*(D^2-d^2)",
        //        new List<VarInfo>()
        //        {
        //            new VarInfo("A", "A", _contentManager.ParamTypes["area"]),
        //            new VarInfo("D", "D", _contentManager.ParamTypes["length"]),
        //            new VarInfo("d", "d", _contentManager.ParamTypes["length"]),
        //        }));

        //    EquationLibrary.AddEquationsToSectionNode(_contentManager, eqnSn, equationDetails);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "C) Solid Bodies"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "D) Arithmetic"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "E) Functions of a circle"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "F) Analytical Geometry"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "G) Statistics"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "H) Differential Calculus"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "I) Integral Calculus"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "J) Differential Equations"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "K) Statics"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "L) Kinematics"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "M) Dynamics"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "N) Hydraulics"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "O) Heat"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "P) Strength"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "Q) Machine Parts"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "R) Production Engineering"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "S) Electrical Engineering"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "T) Control Engineering"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "U) Chemistry"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    sn[0] = new SectionNode(null, "V) Radiation Physics"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------
        //    //sn[0] = new SectionNode(null, "Z) Tables"); equationLibrary.TopSectionNodes.Add(sn[0]);

        //    // ---------------------

        //    return equationLibrary;
        //}


        private static EquationLibrary CreateTestMiscellaneousLibrary()
        {
            IList<EquationDetails> equationDetails = new List<EquationDetails>();

            EquationLibrary equationLibrary = new EquationLibrary()
            {
                Name = "Miscellaneous",
                Description = "",
            };

            SectionNode[] sn = new SectionNode[10];
            SectionNode eqnSn;

            // ---------------------
            sn[0] = new SectionNode(null, "Geometry"); equationLibrary.TopSectionNodes.Add(sn[0]);

            // ----------------
            sn[1] = new SectionNode(sn[0], "Areas");

            // ------------
            sn[2] = new SectionNode(sn[1], "Annulus");

            eqnSn = sn[2];
            equationDetails.Clear();

            //Equation b33: Area of an annulus
            equationDetails.Add(new EquationDetails("Area of an annulus", "A = (PI/4)*(D^2-d^2)",
                new List<VarInfo>()
                {
                    new VarInfo("A", "A", _contentManager.ParamTypes["area"]),
                    new VarInfo("D", "D", _contentManager.ParamTypes["length"]),
                    new VarInfo("d", "d", _contentManager.ParamTypes["length"]),
                }));

            EquationLibrary.AddEquationsToSectionNode(_contentManager, eqnSn, equationDetails);

            // ------------
            sn[2] = new SectionNode(sn[1], "Triangle");

            eqnSn = sn[2];
            equationDetails.Clear();

            //Equation b14: Area of an equilateral triangle
            equationDetails.Add(new EquationDetails("Area of an equilateral triangle", "A = (a^2/4)*Sqrt(3)", null));

            EquationLibrary.AddEquationsToSectionNode(_contentManager, eqnSn, equationDetails);

            // ----------------
            sn[1] = new SectionNode(sn[0], "Solid Bodies");

            // ------------
            sn[2] = new SectionNode(sn[1], "Cylinder");

            eqnSn = sn[2];
            equationDetails.Clear();

            equationDetails.Add(new EquationDetails("Cylinder eq1", "V = PI*r^2*h", null));
            equationDetails.Add(new EquationDetails("Cylinder eq2", "Am = 2*PI*r*h", null));
            equationDetails.Add(new EquationDetails("Cylinder eq3", "A0 = 2*PI*r*(r+h)", null));

            EquationLibrary.AddEquationsToSectionNode(_contentManager, eqnSn, equationDetails);

            // ------------
            sn[2] = new SectionNode(sn[1], "Sphere");

            eqnSn = sn[2];
            equationDetails.Clear();

            // Geometry: Solid Bodies: Sphere with cylindrical boring
            equationDetails.Add(new EquationDetails("Sphere with cylindrical boring", "A0 = 4*PI*Sqrt(((R+r)^3)*(R-r))", null));

            EquationLibrary.AddEquationsToSectionNode(_contentManager, eqnSn, equationDetails);

            // ------------
            sn[2] = new SectionNode(sn[1], "Cone");

            eqnSn = sn[2];
            equationDetails.Clear();

            //Equation c22: Frustrum of a cone, length of a side
            equationDetails.Add(new EquationDetails("Frustrum of a cone, length of a side", "m = Sqrt( ((D-d)/2)^2 + h^2 )", null));

            EquationLibrary.AddEquationsToSectionNode(_contentManager, eqnSn, equationDetails);

            // ------------
            sn[2] = new SectionNode(sn[1], "Ungula");

            eqnSn = sn[2];
            equationDetails.Clear();

            //Equation c43: ungula
            equationDetails.Add(new EquationDetails("c43: ungula", "A0 = Am + (PI/2)*r^2 + (PI/2)*r*Sqrt(r^2+h^2)", null));

            EquationLibrary.AddEquationsToSectionNode(_contentManager, eqnSn, equationDetails);

            // ---------------------
            sn[0] = new SectionNode(null, "Lines and polynomials"); equationLibrary.TopSectionNodes.Add(sn[0]);

            eqnSn = sn[0];
            equationDetails.Clear();

            equationDetails.Add(new EquationDetails("Gradient of a straight line", "m=(y2-y1)/(x2-x1)", null));
            equationDetails.Add(new EquationDetails("Straight line", "y=m*x+c", null));
            equationDetails.Add(new EquationDetails("Positive root of a quadratic equation", "x=(-b+Sqrt(b^2-4*a*c))/(2*a)", null));

            EquationLibrary.AddEquationsToSectionNode(_contentManager, eqnSn, equationDetails);

            // ---------------------
            sn[0] = new SectionNode(null, "Silly"); equationLibrary.TopSectionNodes.Add(sn[0]);

            eqnSn = sn[0];
            equationDetails.Clear();

            equationDetails.Add(new EquationDetails("Silly long one", "m=((y2-y1)/(x2-x1)) + ((-b+Sqrt(b^2-4*a*c))/(2*a+z)) + Sqrt( ((D-d)/2)^2 + h^2 ) + ((f2-f1)/(g2-g1))", null));
            equationDetails.Add(new EquationDetails("Silly division", "m=((y2-y1)/(x2-x1)) / ((f2-f1)/(g2-g1))", null));

            EquationLibrary.AddEquationsToSectionNode(_contentManager, eqnSn, equationDetails);

            // ---------------------
            sn[0] = new SectionNode(null, "Heat Exchangers");
            equationLibrary.TopSectionNodes.Add(sn[0]);

            eqnSn = sn[0];
            equationDetails.Clear();

            equationDetails.Add(new EquationDetails("Heat duty from temperature difference", "Q = m*cp*(T1-T2)", null));
            equationDetails.Add(new EquationDetails("Heat duty from Area", "Q = U*A*DTLM", null));
            equationDetails.Add(new EquationDetails("Log mean temperature difference", "DTLM = (DT1-DT2)/LOG(DT1/DT2)", null));
            equationDetails.Add(new EquationDetails("Log mean temperature difference", "DTLM = ((Thi-Tco)-(Tho-Tci))/LOG((Thi-Tco)/(Tho-Tci))", null));

            EquationLibrary.AddEquationsToSectionNode(_contentManager, eqnSn, equationDetails);

            // ---------------------

            return equationLibrary;
        }


        // -----------------------------
        private static IList<EqnCalc> TestEquationParser()
        {
            IList<EqnCalc> retList = new List<EqnCalc>();

            IList<string> eqnStrings = new List<string>();

            // Engineering: Heat: Heat Exchanger
            // Engineering: Equipment: Heat Exchanger
            eqnStrings.Add("Q = m*cp*(T1-T2)");
            eqnStrings.Add("Q = U*A*DTLM");
            eqnStrings.Add("DTLM = (DT1-DT2)/LOG(DT1/DT2)");
            eqnStrings.Add("DTLM = ((Thi-Tco)-(Tho-Tci))/LOG((Thi-Tco)/(Tho-Tci))");

            // Geometry: Solid Bodies: Cylinder
            eqnStrings.Add("V = PI*r^2*h");
            eqnStrings.Add("Am = 2*PI*r*h");
            eqnStrings.Add("A0 = 2*PI*r*(r+h)");

            foreach (var item in eqnStrings)
            {
                JongErrWarn errW = null;
                FunctionCalc funCalc = _contentManager.CreateFunctionCalcFromExpression(item, null, null, out errW);
                if (funCalc != null)
                {
                    retList.Add((EqnCalc)funCalc);
                }
            }
            return retList;
        }



        #endregion ------------ Test Routines ------------


    }
}
