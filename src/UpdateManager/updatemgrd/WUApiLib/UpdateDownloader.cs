using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[CoClass(typeof(UpdateDownloaderClass))]
[Guid("68F1C6F9-7ECC-4666-A464-247FE12496C3")]
internal interface UpdateDownloader : IUpdateDownloader
{
}
