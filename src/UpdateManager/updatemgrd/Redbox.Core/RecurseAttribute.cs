using System;

namespace Redbox.Core;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
internal class RecurseAttribute : Attribute
{
}
