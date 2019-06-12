This is an unfinished project, which goes partway towards the vision outlined below.
Only the mobile (Xamarin) version is included in this repo, and that is unfinished also.

Screenshots

	Please see the folder "AnyEquation\AnyEquation\Equations\Documents\Android Screenshots"

Coding

	If you are interested in the domain coding, please start looking at the files underneath "AnyEquation\AnyEquation\Equations\Model".
	There is a class diagram in "AnyEquation\AnyEquation\Equations\Documents\Class Diagrams\Calculations.png".
	To understand this rather complicated class structure, I suggest you work outwards from the "FunctionCalc" node in the middle of the diagram.
	A "FunctionCalc" is anything that can take 0 or more arguments and return a single result.
	The inputs to a "FunctionCalc" can be anything that inherits from "SingleResult", including nested FunctionCalc items.
	An equation is a specialized form of "FunctionCalc" that takes two arguments (left and right sides) 
	and returns the difference (hopefully 0).  An equation can also be used to assign the right hand side value to a variable on the left.

	If you are interested in Xamarin stuff, please look at "AnyEquation\AnyEquation\Equations\ViewModels", 
	"AnyEquation\AnyEquation\Equations\Views" and "AnyEquation\AnyEquation.Droid".
	Note that Xamarin is usually used with the MVVM pattern, which is true in this case.

	The equation display takes nice advantage of the Xamarin layout model, whereby you add the items and let them work out themselves where they appear.
	This can be found in "AnyEquation\AnyEquation\Equations\User Controls\ucMathDisplay.xaml.cs".

	The UI uses the "Syncfusion" toolkit, which was a big improvement over just native Xamarin features.


Status and Experience

	I gave up on this project because the amount of remaining work was too great for the likely use it would ever get.
	Also, although Xamarin is potentially very powerful I found the following:

	1) It is not very mature (compared to Windows Forms etc.) and surprisingly buggy.  
	   This also means you are lacking the wealth of online resources and StackOverflow questions you might otherwise find.
	2) It doesn't have a design surface, and I could never get the Xaml preview to work.
	   This means even the simplest UI change is painful unless you are really good at predicting what the flow model will do.
	3) The debugging is limited.  You can put breakpoints and look at variables but it is hard to track down the cause of an 
	   Exception from the information that is available.  Detailed logging is probably the solution here.


Vision
				
	AnyEquation: Any equation, Any Units, Anywhere			
				
	Vision Statement			
		AnyEquation will be an equation-based calculator, with easy selection, or definition, of equations; and supporting any Units of Measure.		
		Equations will be categorized, ranked and attributed, to support easy finding and confident use.		
		It will run on multiple mobile, desktop and web devices.  A key aim should be that it can be used effectively and unobtrusively in meetings.		
		Equations will be maintained by a community of interested users and contributers.		
				
	Background / Introduction			
		This is a new development.		
		There are no obvious competitors at the moment.		
		It is mostly being done as a learning experiece, so the market realities don't matter that much!		
				
Business Case / Positioning				
				
	Opportunity / Unique Selling Point (USP)			
		The main opportunity is for something that can be used effectively and unobtrusively in meetings.		
		This makes a mobile phone APP the most likely important deliverable.		
				
		A key differentiator will be the support for any Units of Measure.  In addition, the units used for a calculation do not have to be consistent.		
		One temperature can be supplied in Centigrade, another in Farenheit.		
		This is crucial for easy use in a meeting scenario, where you need to use any data close to hand.		
		Dimensional analysis will be used to check consistency and perform conversions		
				
		Another differentiator will be the use of a Community to maintain and expand the equation set.		
				
	Problem Statement			
		Technical people need an easy way to calculate things in a meeting, using whatever data is close to hand.		
		A typical scenario is when a group of people are all scratching their heads trying to remember how to do the calculation, 
		and how to convert the data they have into appropriate units.	
		These used to be called "back of the envelope" calculations		
				
	Product Position Statement			
		AnyEquation…		
		<TODO>		
				
	Alternatives and Competition			
		<TODO>		
				
	SWAT			
		<TODO>		
				
				
AIMS				
	Purpose			
		Technical purpose already covered above		
		Commercial purpose:  Unclear at this point!		
				
	Stakeholders			
		Potential Users:		
			Calculation User:	
				Engineers, Scientists, Finance, Accountants, IT, any other numeric technical discipline
			Content Contributer:	
				Vendor, Academics, Calculation Users, Others
				
		Commercial Stakeholders:		
			Vendor	
			Investors	
				
		Development:		
			Ant Waters	
			Collaborators	
				
	Key Features			
		Equation-based calculation		
		Any Units of Measure for Inputs and Results		
		Select equation from a library, or define your own		
		Use details from one calculation as inputs to another		
		Record a History of calculations done; allow them to be re-used etc.		
		Contribute content to your local equation library, or a Community version on the internet		
				
	Business Constraints			
		<TODO>		
				
	Main Risks			
		Probably won't be able to generate any significant revenue from it.  In fact, don't even know how to!		
		It will take too long to do.		
		It will require too many resources.		
		<TODO>		
				
	Aims Grid			
		<TODO>		

