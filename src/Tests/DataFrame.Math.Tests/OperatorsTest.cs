using System;
using System.Collections.Generic;
using Parquet.Data;
using Xunit;
using System.Linq;
using DataFrame.Math.Data;
using DataFrame.Math.Operators;
using Microsoft.DotNet.PlatformAbstractions;

namespace dftest
{
   public class OperatorsTest
   {
      public class Input
      {
         public Input(double expected, Func<IReadOnlyCollection<double>, double> func, params double[] sequence)
         {
            Expected = expected;
            Func = func;
            Sequence = sequence;
         }

         public double Expected { get; }

         public Func<IReadOnlyCollection<double>, double> Func { get; }

         public double[] Sequence { get; }
      }


      public static IEnumerable<object[]> OperatorData = new object[][]
      {
         new object[] { new Input(320.25, i => i.Variance(), 2, 3, 4, 5, 15, 21, 34, 56 ) },
         new object[] { new Input(3, i => i.Mean(), 2, 3, 4) },
         new object[] { new Input(4.06667, i => i.Mean(), 3.2, 4, 5) },
         new object[] { new Input(1, i => i.StandardDeviation(), 2, 3, 4) },
         new object[] { new Input(0.90185, i => i.StandardDeviation(), 3.2, 4, 5) },
         new object[] { new Input(3.5, i => i.Quartile25(), 1, 3, 4, 6, 7, 10, 19, 87) },
         new object[] { new Input(6.5, i => i.Median(), 1, 3, 4, 6, 7, 10, 19, 87) },
         new object[] { new Input(14.5, i => i.Quartile75(), 1, 3, 4, 6, 7, 10, 19, 87) },
         new object[] { new Input(-0.09621, i => i.Skewness(), 5, 20, 40, 80, 100, 102) },
         new object[] { new Input(1.34065, i => i.Kurtosis(), 5, 20, 40, 80, 100, 102) },
      };

      [Theory]
      [MemberData(nameof(OperatorData))]
      public void Operator_calculations_are_correct(Input input)
      {
         double actual = input.Func(input.Sequence);

         Assert.Equal(input.Expected, actual, 5);
      }

      [Fact]
      public void Operator_DotProduct_Possible()
      {
         var lhs = new Matrix<double>(2, 3);
         var rhs = new Matrix<double>(3, 2);

         lhs.AddRow(0, new Series<double>("a", new double[] { 2, 2 }));
         lhs.AddRow(1, new Series<double>("a", new double[] { 2, 2 }));
         lhs.AddRow(2, new Series<double>("a", new double[] { 2, 2 }));

         rhs.AddRow(0, new Series<double>("a", new double[] { 2, 2, 2 }));
         rhs.AddRow(1, new Series<double>("a", new double[] { 2, 2, 2 }));

         Matrix<double> result = lhs.DotProduct(rhs);
         Assert.Equal(result.ColumnCount, 2);
         Assert.Equal(result.RowCount, 2);

         Assert.Equal(result[0, 0], 12);
         Assert.Equal(result[0, 1], 12);
         Assert.Equal(result[1, 0], 12);
         Assert.Equal(result[1, 1], 12);
      }

      [Fact]
      public void Operator_DotProduct_NotPossible()
      {
         var lhs = new Matrix<double>(3, 2);
         var rhs = new Matrix<double>(3, 2);

         Assert.Throws<Exception>(() => lhs.DotProduct(rhs));
      }

      [Fact]
      public void Operator_IdentityMatrix()
      {
         /*var lhs = new Matrix<double>(2, 2);
         lhs.AddRow(0, new Series<double>("a", new double[] { 1, 0 }));
         lhs.AddRow(1, new Series<double>("b", new double[] { 0, 1 }));
         */

         Matrix<double> identity = new Matrix<double>(2, 2).IdentityMatrix();
            //lhs.IdentityMatrix;
         Assert.Equal(identity.ColumnCount, 2);
         Assert.Equal(identity.RowCount, 2);

         Assert.Equal(identity[0, 0], 1);
         Assert.Equal(identity[0, 1], 0);
         Assert.Equal(identity[1, 0], 0);
         Assert.Equal(identity[1, 1], 1);

      }

      [Fact]
      public void Operator_Transpose()
      {
         var mat = new Matrix<double>(3, 2);
         mat.AddRow(0, new Series<double>("a", new double[] { 1, 2, 3 }));
         mat.AddRow(1, new Series<double>("b", new double[] { 4, 5, 6 }));

         Matrix<double> transposed = mat.Transpose();
         Assert.Equal(transposed.ColumnCount, mat.RowCount);
         Assert.Equal(transposed.RowCount, mat.ColumnCount);

         Assert.Equal(transposed[0, 0], mat[0,0]);
         Assert.Equal(transposed[0, 1], mat[1,0]);
         Assert.Equal(transposed[1, 0], mat[0,1]);
         Assert.Equal(transposed[1, 1], mat[1,1]);

      }

      [Fact]

      public void Operator_InverseMatrix()
      {
         var mat = new Matrix<double>(3, 3);
         mat.AddRow(0, new Series<double>("a", new double[] { 1, 2, 3 }));
         mat.AddRow(1, new Series<double>("b", new double[] { 4, 5, 6 }));
         mat.AddRow(2, new Series<double>("c", new double[] { 7, 8, 9 }));

         Matrix<double> minor = mat.GetMatrixMinor(0,0);
         Assert.Equal(minor.RowCount, mat.RowCount-1);
         Assert.Equal(minor.ColumnCount, mat.ColumnCount-1);

         Assert.Equal(minor[0, 0], mat[1,1]);
         Assert.Equal(minor[0, 1], mat[1,2]);
         Assert.Equal(minor[1, 0], mat[2,1]);
         Assert.Equal(minor[1, 1], mat[2,2]);

         Assert.Equal(minor.GetMatrixDeterminant(), 5 * 9 - 6 * 8);
         Assert.Equal(mat.GetMatrixDeterminant(), 0); // since R2 is twice R1
         //Assert.Throws<Exception>(() => mat.InverseMatrix());

         // new non-singular matrix
         mat.AddRow(0, new Series<double>("a", new double[] { 1, 2, 3 }));
         mat.AddRow(1, new Series<double>("b", new double[] { 2, 3, 1 }));
         mat.AddRow(2, new Series<double>("c", new double[] { 3, 1, 2 }));

         //identity matrix
         Matrix<double> identity = new Matrix<double>(3, 3).IdentityMatrix();

         Assert.Equal(mat.InverseMatrix().DotProduct(mat).GetMatrixDeterminant().ToString(), "1");


      }
   }
}
