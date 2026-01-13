#if NETSTANDARD
using System.ComponentModel;


//DO NOT TOUCH HACK FOR THE RECORDS SUPPORT
namespace System.Runtime.CompilerServices;

[EditorBrowsable(EditorBrowsableState.Never)]
public class IsExternalInit
{
}

#endif