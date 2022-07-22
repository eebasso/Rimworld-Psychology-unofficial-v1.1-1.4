using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Verse;
//using Accord.Math;
//using Accord.Math.Decompositions;
//using System.Numerics;
using UnityEngine;
//using MathNet.Numerics.LinearAlgebra;
//using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Diagnostics;


namespace Psychology
{
    [StaticConstructorOnStartup]
    public class PersonalityNodeParentMatrix
    {
        public static List<PersonalityNodeDef> defList;
        public static Dictionary<PersonalityNodeDef, int> indexDict;
        //public static float[] parentModifierMatrix;
        //public static float[] parentTransformMatrix;
        //public static Matrix<float> parentModifierMatrix;
        //public static Matrix<float> parentTransformMatrix;
        public static float[] parentModifierMatrix;
        public static float[] parentTransformMatrix;

        static PersonalityNodeParentMatrix()
        {
            Log.Message("Initializing PersonalityNodeParentMatrix");
            defList = DefDatabase<PersonalityNodeDef>.AllDefsListForReading;
            Log.Message("Initializing indexDict");
            indexDict = new Dictionary<PersonalityNodeDef, int>();
            Log.Message("Define n");
            int order = defList.Count();
            Log.Message("Initialize parentModifierMatrix");
            //parentModifierMatrix = Matrix.Identity<float>(n);

            parentModifierMatrix = IdentityMatrix(order);
            Log.Message("parentModifierMatrix[3, 3] = " + parentModifierMatrix[3 + 3 * order]);
            Log.Message("parentModifierMatrix[4, 5] = " + parentModifierMatrix[4 + 5 * order]);
            int index = 0;

            float[] testMatrix = { 0f, 1f, 1f, 0f };
            (float[] testV, float[] testD) = EigenDecomp(testMatrix, 2);
            float[] testVDVT = VtimesDtimesTransposeOfV(testV, testD, 2);
            Log.Message("Diagonal matrix D = " + string.Join(", ", testD));
            Log.Message("Eigenvector matrix V= " + string.Join(", ", testV));
            Log.Message("V * D * V^T = " + string.Join(", ", testVDVT));
            float[] M = { 1f, 2f, 3f, 4f };
            float[] vec = { 5f, 6f };
            float[] MtimesVec = MatrixVectorProduct(M, vec, 2);
            Log.Message("MtimesVec = " + string.Join(", ", MtimesVec));

            foreach (PersonalityNodeDef def in defList)
            {
                //Log.Message("indexDict gained element: (" + index + ", " + def.defName + ")");
                indexDict.Add(def, index);
                index++;
            }
            //Log.Message("Adding elements to parentModifierMatrix");
            foreach (PersonalityNodeDef def in defList)
            {
                int i = indexDict[def];
                //Log.Message("Element: (" + i + ", " + def.defName + ")");
                foreach (KeyValuePair<PersonalityNodeDef, PersonalityNodeParent> kvp in def.ParentNodes)
                {
                    int k = indexDict[kvp.Key];
                    float modifier = kvp.Value.modifier;
                    parentModifierMatrix[i + k * order] += 2 * modifier;
                    parentModifierMatrix[k + i * order] += 2 * modifier;
                }
            }
            Log.Message("Start Decomposition");
            float[] C = parentModifierMatrix;
            (float[] V, float[] d) = EigenDecomp(C, order);
            bool flag = false;
            int ticker = 0;
            int size = order * order;
            while (flag == false && ticker < 500)
            {
                flag = true;
                ticker++;
                for (int diagIndex = 0; diagIndex < size; diagIndex += order + 1)
                {
                    //if (C[diagIndex] != 1f)
                    if (Mathf.Abs(C[diagIndex] - 1f) > 1e-4f)
                    {
                        C[diagIndex] = 1f;
                        flag = false;
                    }
                }
                (V, d) = EigenDecomp(C, order);
                for (int i = 0; i < order; ++i)
                {
                    if (d[i] < 0f)
                    {
                        d[i] = 0f;
                        flag = false;
                    }
                }
                if (flag == false)
                {
                    C = VtimesDtimesTransposeOfV(V, d, order);
                }
            }
            Log.Message("ticker = " + ticker);
            Log.Message("Eigenvalues = " + String.Join(", ", d));
            string diagonalElements = "Diagonal elements of C = " + C[0];
            for (int diagIndex = order + 1; diagIndex < size; diagIndex += order + 1)
            {
                diagonalElements += ", " + C[diagIndex];
            }
            Log.Message(diagonalElements);


            //for (int i = 0; i < order; i++)
            //{
            //    Log.Message("Row = " + i);
            //    string rowOfM = "Row " + i + " of M = " + parentModifierMatrix[i];
            //    string rowOfC = "Row " + i + " of C = " + C[i];
            //    for (int j = 1; j < order; ++j)
            //    {
            //        rowOfM += ", " + parentModifierMatrix[i + j * order];
            //        rowOfC += ", " + C[i + j * order];
            //    }
            //    Log.Message(rowOfM);
            //    Log.Message(rowOfC);
            //}





            //float[] Dsqrt = Matrix.Identity<float>(n);
            //Matrix<float> Dsqrt = Matrix<float>.Build.DenseDiagonal(n, 1f);
            float[] dSqrt = new float[order];
            for (int i = 0; i < order; ++i)
            {
                dSqrt[i] = Mathf.Sqrt(Mathf.Abs(d[i]));
            }
            //parentTransformMatrix = V.Dot(Dsqrt).Dot(V.Inverse());
            //parentTransformMatrix = MatrixProduct(V, MatrixProduct(Dsqrt, MatrixInverse(V, order), order), order);
            parentTransformMatrix = VtimesDtimesTransposeOfV(V, dSqrt, order);
            Log.Message("PersonalityNodeParentMatrix initialized");

            foreach (PersonalityNodeDef def in defList)
            {
                int i = indexDict[def];
                string rowOfMCP = def.defName + ": ";
                foreach (PersonalityNodeDef def2 in defList)
                {
                    int j = indexDict[def2];
                    float num1 = parentModifierMatrix[i + j * order];
                    float num2 = C[i + j * order];
                    float num3 = parentTransformMatrix[i + j * order];
                    rowOfMCP += " {" + def2.defName + " " + num1 + ", " + num2 + ", " + num3 + "},";
                }
                Log.Message(rowOfMCP);
            }
        }

        public static (int, int) Get2Dindicies(int s, int order)
        {
            //Log.Message("s = " + s + " i,j = " + s / order + ", " + s % order);
            return (s % order, s / order);
        }

        /// <summary>
        /// Computes the eigenvalues and eigenvectors of a matrix.
        /// </summary>
        /// <param name="isSymmetric">Whether the matrix is symmetric or not.</param>
        /// <param name="order">The order of the matrix.</param>
        /// <param name="matrix">The matrix to decompose. The length of the array must be order * order.</param>
        /// <param name="matrixV">On output, the matrix contains the eigen vectors. The length of the array must be order * order.</param>
        /// <param name="vectorValues">On output, the eigen values (λ) of matrix in ascending value. The length of the array must be order.</param>
        // public void EigenDecomp(int order, float[] matrix, float[] matrixEv, Complex[] vectorEv, float[] matrixD)
        public static (float[], float[]) EigenDecomp(float[] matrix, int order)
        {
            float[] matrixV = new float[order * order];
            float[] vectorValues = new float[order];
            float[] array2 = new float[order];

            Buffer.BlockCopy(matrix, 0, matrixV, 0, matrix.Length * 4);
            int num = order - 1;
            for (int i = 0; i < order; ++i)
            {
                vectorValues[i] = matrixV[num + i * order];
            }

            (matrixV, vectorValues, array2) = SymmetricTridiagonalize(matrixV, vectorValues, array2, order);
            (matrixV, vectorValues, array2) = SymmetricDiagonalize(matrixV, vectorValues, array2, order);

            for (int jj = 0; jj < order * order; jj += order)
            {
                float norm = matrixV[jj] * matrixV[jj];
                for (int i = 1; i < order; ++i)
                {
                    norm += matrixV[i + jj] * matrixV[i + jj];
                }
                for (int i = 0; i < order; ++i)
                {
                    matrixV[i + jj] /= norm;
                }
            }
            return (matrixV, vectorValues);
        }

        /// <summary>
        /// Symmetric Householder reduction to tridiagonal form.
        /// </summary>
        /// <param name="eigenVectors">The eigen vectors to work on.</param>
        /// <param name="d">Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures tred2 by
        /// Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        /// <summary>
        /// Symmetric Householder reduction to tridiagonal form.
        /// </summary>
        /// <param name="a">Data array of matrix V (eigenvectors)</param>
        /// <param name="d">Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures tred2 by
        /// Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        ///
        public static (float[], float[], float[]) SymmetricTridiagonalize(float[] a, float[] d, float[] e, int order)
        {
            //Log.Message("SymmetricTridiagonalize");
            // Householder reduction to tridiagonal form.
            for (var i = order - 1; i > 0; --i)
            {
                // Scale to avoid under/overflow.
                var scale = 0.0f;
                var h = 0.0f;

                for (var k = 0; k < i; ++k)
                {
                    scale = scale + Math.Abs(d[k]);
                }

                if (scale == 0.0f)
                {
                    e[i] = d[i - 1];
                    for (var j = 0; j < i; ++j)
                    {
                        d[j] = a[(j * order) + i - 1];
                        a[(j * order) + i] = 0.0f;
                        a[(i * order) + j] = 0.0f;
                    }
                }
                else
                {
                    // Generate Householder vector.
                    for (var k = 0; k < i; ++k)
                    {
                        d[k] /= scale;
                        h += d[k] * d[k];
                    }

                    var f = d[i - 1];
                    var g = (float)Math.Sqrt(h);
                    if (f > 0)
                    {
                        g = -g;
                    }

                    e[i] = scale * g;
                    h = h - (f * g);
                    d[i - 1] = f - g;

                    for (var j = 0; j < i; ++j)
                    {
                        e[j] = 0.0f;
                    }

                    // Apply similarity transformation to remaining columns.
                    for (var j = 0; j < i; ++j)
                    {
                        f = d[j];
                        a[(i * order) + j] = f;
                        g = e[j] + (a[(j * order) + j] * f);

                        for (var k = j + 1; k <= i - 1; ++k)
                        {
                            g += a[(j * order) + k] * d[k];
                            e[k] += a[(j * order) + k] * f;
                        }

                        e[j] = g;
                    }

                    f = 0.0f;

                    for (var j = 0; j < i; ++j)
                    {
                        e[j] /= h;
                        f += e[j] * d[j];
                    }

                    var hh = f / (h + h);

                    for (var j = 0; j < i; ++j)
                    {
                        e[j] -= hh * d[j];
                    }

                    for (var j = 0; j < i; ++j)
                    {
                        f = d[j];
                        g = e[j];

                        for (var k = j; k <= i - 1; ++k)
                        {
                            a[(j * order) + k] -= (f * e[k]) + (g * d[k]);
                        }

                        d[j] = a[(j * order) + i - 1];
                        a[(j * order) + i] = 0.0f;
                    }
                }

                d[i] = h;
            }

            // Accumulate transformations.
            for (var i = 0; i < order - 1; ++i)
            {
                a[(i * order) + order - 1] = a[(i * order) + i];
                a[(i * order) + i] = 1.0f;
                var h = d[i + 1];
                if (h != 0.0f)
                {
                    for (var k = 0; k <= i; ++k)
                    {
                        d[k] = a[((i + 1) * order) + k] / h;
                    }

                    for (var j = 0; j <= i; ++j)
                    {
                        var g = 0.0f;
                        for (var k = 0; k <= i; ++k)
                        {
                            g += a[((i + 1) * order) + k] * a[(j * order) + k];
                        }

                        for (var k = 0; k <= i; ++k)
                        {
                            a[(j * order) + k] -= g * d[k];
                        }
                    }
                }

                for (var k = 0; k <= i; ++k)
                {
                    a[((i + 1) * order) + k] = 0.0f;
                }
            }

            for (var j = 0; j < order; ++j)
            {
                d[j] = a[(j * order) + order - 1];
                a[(j * order) + order - 1] = 0.0f;
            }

            a[(order * order) - 1] = 1.0f;
            e[0] = 0.0f;
            return (a, d, e);
        }

        /// <summary>
        /// Symmetric tridiagonal QL algorithm.
        /// </summary>
        /// <param name="a">Data array of matrix V (eigenvectors)</param>
        /// <param name="d">Arrays for internal storage of real parts of eigenvalues</param>
        /// <param name="e">Arrays for internal storage of imaginary parts of eigenvalues</param>
        /// <param name="order">Order of initial matrix</param>
        /// <remarks>This is derived from the Algol procedures tql2, by
        /// Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
        /// Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.</remarks>
        public static (float[], float[], float[]) SymmetricDiagonalize(float[] a, float[] d, float[] e, int order)
        {
            const int Maxiter = 1000;
            //Log.Message("SymmetricDiagonalize");
            for (var i = 1; i < order; i++)
            {
                e[i - 1] = e[i];
            }

            e[order - 1] = 0.0f;

            var f = 0.0f;
            var tst1 = 0.0f;
            var eps = 1e-9f;

            //Log.Message("Second for loop");
            for (var l = 0; l < order; l++)
            {
                // Find small subdiagonal element
                tst1 = Math.Max(tst1, Math.Abs(d[l]) + Math.Abs(e[l]));
                var m = l;
                while (m < order)
                {
                    if (Math.Abs(e[m]) <= eps * tst1)
                    {
                        break;
                    }

                    m++;
                }

                // If m == l, d[l] is an eigenvalue,
                // otherwise, iterate.
                if (m > l)
                {
                    var iter = 0;
                    do
                    {
                        iter = iter + 1; // (Could check iteration count here.)

                        // Compute implicit shift
                        var g = d[l];
                        var p = (d[l + 1] - g) / (2.0f * e[l]);
                        var r = Hypotenuse(p, 1.0f);
                        if (p < 0)
                        {
                            r = -r;
                        }

                        d[l] = e[l] / (p + r);
                        d[l + 1] = e[l] * (p + r);

                        var dl1 = d[l + 1];
                        var h = g - d[l];
                        for (var i = l + 2; i < order; ++i)
                        {
                            d[i] -= h;
                        }

                        f = f + h;

                        // Implicit QL transformation.
                        p = d[m];
                        var c = 1.0f;
                        var c2 = c;
                        var c3 = c;
                        var el1 = e[l + 1];
                        var s = 0.0f;
                        var s2 = 0.0f;
                        for (var i = m - 1; i >= l; i--)
                        {
                            c3 = c2;
                            c2 = c;
                            s2 = s;
                            g = c * e[i];
                            h = c * p;
                            r = Hypotenuse(p, e[i]);
                            e[i + 1] = s * r;
                            s = e[i] / r;
                            c = p / r;
                            p = (c * d[i]) - (s * g);
                            d[i + 1] = h + (s * ((c * g) + (s * d[i])));

                            // Accumulate transformation.
                            for (var k = 0; k < order; ++k)
                            {
                                h = a[((i + 1) * order) + k];
                                a[((i + 1) * order) + k] = (s * a[(i * order) + k]) + (c * h);
                                a[(i * order) + k] = (c * a[(i * order) + k]) - (s * h);
                            }
                        }

                        p = (-s) * s2 * c3 * el1 * e[l] / dl1;
                        e[l] = s * p;
                        d[l] = c * p;

                        // Check for convergence. If too many iterations have been performed, 
                        // throw exception that Convergence Failed
                        if (iter >= Maxiter)
                        {
                            Log.Message("Convergence failed");
                        }
                    } while (Math.Abs(e[l]) > eps * tst1);
                }

                d[l] = d[l] + f;
                e[l] = 0.0f;
            }

            //Log.Message("Third for loop");
            // Sort eigenvalues and corresponding vectors.
            for (var i = 0; i < order - 1; ++i)
            {
                var k = i;
                var p = d[i];
                for (var j = i + 1; j < order; ++j)
                {
                    if (d[j] < p)
                    {
                        k = j;
                        p = d[j];
                    }
                }

                if (k != i)
                {
                    d[k] = d[i];
                    d[i] = p;
                    for (var j = 0; j < order; ++j)
                    {
                        p = a[(i * order) + j];
                        a[(i * order) + j] = a[(k * order) + j];
                        a[(k * order) + j] = p;
                    }
                }
            }
            return (a, d, e);
        }

        /// <summary>
        /// Numerically stable hypotenuse of a right angle triangle, i.e. <code>(a,b) -&gt; sqrt(a^2 + b^2)</code>
        /// </summary>
        /// <param name="a">The length of side a of the triangle.</param>
        /// <param name="b">The length of side b of the triangle.</param>
        /// <returns>Returns <code>sqrt(a<sup>2</sup> + b<sup>2</sup>)</code> without underflow/overflow.</returns>
        public static float Hypotenuse(float a, float b)
        {
            if (Math.Abs(a) > Math.Abs(b))
            {
                float num = b / a;
                return Math.Abs(a) * (float)Math.Sqrt(1f + num * num);
            }
            if ((float)b != 0.0)
            {
                float num2 = a / b;
                return Math.Abs(b) * (float)Math.Sqrt(1f + num2 * num2);
            }
            return 0f;
        }

        public static float[] VtimesDtimesTransposeOfV(float[] V, float[] d, int order)
        {
            int size = order * order;
            float[] matrix = new float[size];
            for (int s = 0; s < size; ++s)
            {
                (int i, int j) = Get2Dindicies(s, order);
                for (int k = 0; k < order; ++k)
                {
                    matrix[s] += V[i + k * order] * d[k] * V[j + k * order];
                    //matrix[1] += V[1 + 2 k] * d[k] * V[0 + 2k];
                    //matrix[1] += V[1] * d[0] * V[0] + V[3] * d[1] * V[1];
                }
            }
            return matrix;
        }

        // --------------------------------------------------

        public static float[] IdentityMatrix(int order)
        {
            // return an order x order Identity matrix
            float[] identity = new float[order * order];
            int i;
            int j;
            for (int s = 0; s < order * order; ++s)
            {
                (i, j) = Get2Dindicies(s, order);
                identity[s] = i == j ? 1f : 0f;
            }
            return identity;
        }

        // --------------------------------------------------

        //public static float[] MatrixProduct(float[] A, float[] B, int order)
        //{
        //    float[] C = new float[order * order];
        //    for (int s = 0; s < order * order; ++s)
        //    {
        //        //(int i, int j) = Get2Dindicies(s, order);
        //        int j = s % order;
        //        float num = 0f;
        //        for (int k = 0; k < order; ++k)
        //        {
        //            num += A[s - j + k] * B[order * k + j];
        //        }
        //        C[s] = num;
        //    }
        //    return C;
        //}

        // --------------------------------------------------

        // result of multiplying an order x order matrix by a order x 1 column vector (yielding an order x 1 column vector)
        public static float[] MatrixVectorProduct(float[] A, float[] b, int order)
        {
            float[] c = new float[order];
            //float sum;
            //for (int i = 0; i < order; ++i)
            //{
            //    sum = 0f;
            //    int jj = 0;
            //    for (int j = 0; j < order; ++j)
            //    {
            //        sum += A[i + jj] * b[j];
            //        jj += order;
            //    }
            //    c[i] = sum;
            //    //c[0] = A[0 + 0] * b[0] + A[2] * b[1]

            //}
            //for (int i = 0; i < order; ++i)
            //{
            //    c[i] = A[order * i] * b[0];
            //}

            for (int s = order; s < order * order; ++s)
            {
                c[s % order] += A[s] * b[s / order];
            }
            //int j = -1;
            //for (int s = 0; s < order * order; ++s)
            //{
            //    int i = s % order;
            //    if (i == 0)
            //    {
            //        j += 1;
            //    }
            //    c[i] += A[s] * b[j];
            //}
            return c;
        }

        //public float[] ConvertMatrixToArray(float[,] matrix)
        //{
        //    int order = matrix.GetLength(0);
        //    float[] matrixArray = new float[order * order];
        //    for (int i = 0; i < order; ++i)
        //    {
        //        for (int j = 0; j < order; ++j)
        //        {
        //            matrixArray[i + j * order] = matrix[i, j];
        //        }
        //    }
        //    return matrixArray;
        //}

        //public float[,] ConvertArrayToMatrix(float[] matrixArray)
        //{
        //    int order = (int)Math.Sqrt(matrixArray.Count());
        //    float[,] matrix = new float[order, order];
        //    for (int i = 0; i < order; ++i)
        //    {
        //        for (int j = 0; j < order; ++j)
        //        {
        //            matrix[i, j] = matrixArray[i + j * order];
        //        }
        //    }
        //    return matrix;
        //}

        // --------------------------------------------------

        //public static float[] MatrixDecompose(float[] matrix, out int[] perm, out int toggle, int order)
        //{
        //    // Doolittle LUP decomposition with partial pivoting.
        //    // rerturns: result is L (with 1s on diagonal) and U;
        //    // perm holds row permutations; toggle is +1 or -1 (even or odd)
        //    float[] result = matrix;
        //    perm = new int[order]; // set up row permutation result
        //    for (int i = 0; i < order; ++i)
        //    {
        //        perm[i] = i;
        //    }

        //    toggle = 1; // toggle tracks row swaps.
        //                // +1 -greater-than even, -1 -greater-than odd. used by MatrixDeterminant

        //    float[] rowPtr = new float[order];
        //    for (int j = 0; j < order - 1; ++j) // each column
        //    {
        //        float colMax = Math.Abs(result[j + j * order]); // find largest val in col
        //        int pRow = j;
        //        //for (int i = j + 1; i less-than n; ++i)
        //        //{
        //        //  if (result[i + j * order] greater-than colMax)
        //        //  {
        //        //    colMax = result[i + j * order];
        //        //    pRow = i;
        //        //  }
        //        //}

        //        // reader Matt V needed this:
        //        for (int i = j + 1; i < order; ++i)
        //        {
        //            if (Math.Abs(result[i + j * order]) > colMax)
        //            {
        //                colMax = Math.Abs(result[i + j * order]);
        //                pRow = i;
        //            }
        //        }
        //        // Not sure if this approach is needed always, or not.

        //        if (pRow != j) // if largest value not on pivot, swap order
        //        {
        //            for (int k = 0; k < order; ++k)
        //            {
        //                int kCol = k * order;
        //                rowPtr[k] = result[pRow + kCol];
        //                result[pRow + kCol] = result[j + kCol];
        //                result[j + kCol] = rowPtr[k];
        //            }
        //            //rowPtr = result[pRow];
        //            //result[pRow] = result[j];
        //            //result[j] = rowPtr;

        //            int tmp = perm[pRow]; // and swap perm info
        //            perm[pRow] = perm[j];
        //            perm[j] = tmp;

        //            toggle = -toggle; // adjust the row-swap toggle
        //        }

        //        // --------------------------------------------------
        //        // This part added later (not in original)
        //        // and replaces the 'return null' below.
        //        // if there is a 0 on the diagonal, find a good row
        //        // from i = j+1 down that doesn't have
        //        // a 0 in column j, and swap that good row with row j
        //        // --------------------------------------------------

        //        if (result[j + j * order] == 0f)
        //        {
        //            // find a good row to swap
        //            int goodRow = -1;
        //            for (int row = j + 1; row < order; ++row)
        //            {
        //                if (result[row + j * order] != 0f)
        //                {
        //                    goodRow = row;
        //                }
        //            }
        //            if (goodRow == -1)
        //            {
        //                throw new Exception("Cannot use Doolittle's method");
        //            }
        //            // swap order so 0.0 no longer on diagonal
        //            //float[] rowPtr = result[goodRow];
        //            //result[goodRow] = result[j];
        //            //result[j] = rowPtr;
        //            for (int k = 0; k < order; ++k)
        //            {
        //                int kCol = k * order;
        //                rowPtr[k] = result[goodRow];
        //                result[goodRow + kCol] = result[j + kCol];
        //                result[j + kCol] = rowPtr[k];
        //            }
        //            int tmp = perm[goodRow]; // and swap perm info
        //            perm[goodRow] = perm[j];
        //            perm[j] = tmp;

        //            toggle = -toggle; // adjust the row-swap toggle
        //        }
        //        // --------------------------------------------------
        //        // if diagonal after swap is zero . .
        //        //if (Math.Abs(result[j + j * order]) less-than 1.0E-20) 
        //        //  return null; // consider a throw

        //        for (int i = j + 1; i < order; ++i)
        //        {
        //            result[i + j * order] /= result[j + j * order];
        //            for (int k = j + 1; k < order; ++k)
        //            {
        //                result[i + k * order] -= result[i + j * order] * result[j + k * order];
        //            }
        //        }

        //    } // main j column loop

        //    return result;
        //} // MatrixDecompose

        // --------------------------------------------------

        //public static float[] MatrixInverse(float[] matrix, int order)
        //{
        //    float[] result = matrix;

        //    int[] perm;
        //    int toggle;
        //    float[] lum = MatrixDecompose(matrix, out perm, out toggle, order);
        //    if (lum == null)
        //        throw new Exception("Unable to compute inverse");

        //    float[] b = new float[order];
        //    for (int i = 0; i < order; ++i)
        //    {
        //        for (int j = 0; j < order; ++j)
        //        {
        //            b[j] = i == perm[j] ? 1f : 0f;
        //        }

        //        float[] x = HelperSolve(lum, b, order); // 

        //        for (int j = 0; j < order; ++j)
        //        {
        //            result[j + i * order] = x[j];
        //        }
        //    }
        //    return result;
        //}

        // --------------------------------------------------

        //static float MatrixDeterminant(float[] matrix, int order)
        //{
        //    int[] perm;
        //    int toggle;
        //    float[] lum = MatrixDecompose(matrix, out perm, out toggle, order);
        //    if (lum == null)
        //        throw new Exception("Unable to compute MatrixDeterminant");
        //    float result = toggle;
        //    for (int i = 0; i < lum.Length; ++i)
        //        result *= lum[i + i * order];
        //    return result;
        //}

        // --------------------------------------------------

        //public static float[] HelperSolve(float[] luMatrix, float[] b, int order)
        //{
        //    // before calling this helper, permute b using the perm array
        //    // from MatrixDecompose that generated luMatrix
        //    float[] x = new float[order];
        //    b.CopyTo(x, 0);

        //    for (int i = 1; i < order; ++i)
        //    {
        //        float sum = x[i];
        //        for (int j = 0; j < i; ++j)
        //        {
        //            sum -= luMatrix[i + j * order] * x[j];
        //        }
        //        x[i] = sum;
        //    }

        //    x[order - 1] /= luMatrix[order * order - 1];
        //    for (int i = order - 2; i >= 0; --i)
        //    {
        //        float sum = x[i];
        //        for (int j = i + 1; j < order; ++j)
        //        {
        //            sum -= luMatrix[i + j * order] * x[j];
        //        }
        //        x[i] = sum / luMatrix[i + i * order];
        //    }

        //    return x;
        //}

        // --------------------------------------------------

        //static float[] SystemSolve(float[] A, float[] b, int order)
        //{
        //    // Solve Ax = b

        //    // 1. decompose A
        //    int[] perm;
        //    int toggle;
        //    float[] luMatrix = MatrixDecompose(A, out perm, out toggle, order);
        //    if (luMatrix == null)
        //    {
        //        return null;
        //    }


        //    // 2. permute b according to perm[] into bp
        //    float[] bp = new float[order];
        //    for (int i = 0; i < order; ++i)
        //    {
        //        bp[i] = b[perm[i]];
        //    }


        //    // 3. call helper
        //    float[] x = HelperSolve(luMatrix, bp, order);
        //    return x;
        //} // SystemSolve

        // --------------------------------------------------

        //static float[] MatrixDuplicate(float[] matrix)
        //{
        //    // allocates/creates a duplicate of a matrix.
        //    float[] result = new float[matrix.Length, matrix.GetLength(1));
        //    for (int i = 0; i < matrix.Length; ++i) // copy the values
        //        for (int j = 0; j < matrix[i].Length; ++j)
        //            result[i + j * order] = matrix[i + j * order];
        //    return result;
        //}

    }
}

