using Redbox.Core;

namespace Redbox.Messaging;

internal class MessagingResult
{
	private Redbox.Messaging.ErrorList m_errors;

	public bool Success { get; set; }

	public Redbox.Messaging.ErrorList Errors
	{
		get
		{
			if (m_errors == null)
			{
				m_errors = new Redbox.Messaging.ErrorList();
			}
			return m_errors;
		}
	}

	public void AddError(Redbox.Messaging.Error error)
	{
		Errors.Add(error);
		LogHelper.Instance.Log(string.Format("{0} Code: {1}, Description: {2}{3}", error.IsWarning ? "Warning" : "Error", error.Code, error.Description, string.IsNullOrEmpty(error.Details) ? string.Empty : (", Details: " + error.Details)));
	}

	public void AddErrorList(Redbox.Messaging.ErrorList errorList)
	{
		Errors.AddRange(errorList);
	}
}
