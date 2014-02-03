using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

internal class MatrixSelector
{
	private class MatrixData
	{
		internal string Matrix { get; set; }

		internal int Count { get; set; }
	}

	internal static MatrixSelector Instance => Singleton<MatrixSelector>.Instance;

	private MatrixSelector()
	{
	}

	private string Decide(string[] matrices)
	{
		if (matrices.Length == 1)
		{
			return matrices[0];
		}
		List<MatrixData> list = new List<MatrixData>();
		try
		{
			string matrix;
			for (int i = 0; i < matrices.Length; i++)
			{
				matrix = matrices[i];
				MatrixData matrixData = list.Find((MatrixData each) => each.Matrix == matrix);
				if (matrixData == null)
				{
					list.Add(new MatrixData
					{
						Count = 1,
						Matrix = matrix
					});
				}
				else
				{
					matrixData.Count++;
				}
				list.Sort((MatrixData x, MatrixData y) => y.Count - x.Count);
			}
			if (list.Count == 1)
			{
				return list[0].Matrix;
			}
			return (list[0].Count > list[1].Count) ? list[0].Matrix : "UNKNOWN";
		}
		finally
		{
			list.Clear();
		}
	}
}
