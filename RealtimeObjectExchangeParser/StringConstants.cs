namespace ROXParser
{
  public class StringConstants
  {
    // program title
    public const string ProgramTitle = "*** Realtime Object Parser v:0.1  Copyright by Laszlo Arvai 2012-2015 ***\n";

    // usage messages
    public const string Usage = "Usage: SettingsParser <-switches> <switchfile>\n The 'switchfile' is a text file and one line of the file must contain one\n command line switch.\n Supported switches:\n -help or -? - Displays help message\n -packetdecl:<packetdeclaration> - Specifies packet declaration file name\n -cdecl:<headerfilename> - Creates C style header file using the given name\n -types:<typefilename> - Specifies the file name of the type declaration file\n";

		// error messages
		public const string ErrorInvalidElementType = "Invalid element type. ({0})";

  }
}