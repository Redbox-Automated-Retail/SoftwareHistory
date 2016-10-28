using System.Runtime.InteropServices;

namespace Alsing.SourceCode;

[ComVisible(true)]
public delegate void ParserEventHandler(object sender, RowEventArgs e);
