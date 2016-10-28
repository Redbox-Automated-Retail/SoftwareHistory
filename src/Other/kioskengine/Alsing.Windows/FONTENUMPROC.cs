using System.Runtime.InteropServices;

namespace Alsing.Windows;

[ComVisible(true)]
public delegate int FONTENUMPROC(ENUMLOGFONTEX f, int lpntme, int FontType, int lParam);
