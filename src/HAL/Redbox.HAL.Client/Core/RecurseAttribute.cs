using System;

namespace Redbox.HAL.Core;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
internal class RecurseAttribute : Attribute
{
}
