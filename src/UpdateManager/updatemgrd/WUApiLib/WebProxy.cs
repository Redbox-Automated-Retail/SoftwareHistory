using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[CoClass(typeof(WebProxyClass))]
[Guid("174C81FE-AECD-4DAE-B8A0-2C6318DD86A8")]
internal interface WebProxy : IWebProxy
{
}
