using System;

namespace Redbox.WUA;

internal class UpdateError
{
	private ErrorList _errors = new ErrorList();

	public Guid Id { get; set; }

	public UpdateOperationResultCode UpdateOperationResultCode { get; set; }

	public ErrorList Errors => _errors;
}
