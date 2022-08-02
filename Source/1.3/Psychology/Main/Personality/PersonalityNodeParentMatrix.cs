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
        public static Dictionary<int, float[]> PartiaProjectionMatrixDict;
        public static float[] parentModifierMatrix;
        public static float[] parentTransformMatrix;
        public static int order;
        public static int size;
        public static List<float[]> bigFiveVectors = new List<float[]>();
        public static float[] bigFiveStandardDevInvs = new float[5];

        static PersonalityNodeParentMatrix()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //Log.Message("Initializing PersonalityNodeParentMatrix");
            defList = DefDatabase<PersonalityNodeDef>.AllDefsListForReading;

            //Log.Message("Initializing indexDict");
            indexDict = new Dictionary<PersonalityNodeDef, int>();

            //Log.Message("Define order and size");
            order = defList.Count();
            size = order * order;

            //float[] a = { 0f, 1f, 2f, 0f };
            //float[] x = { 0f, 1f, 2f, 0f };
            //float[] y = { 0f, 1f, 2f, 0f };
            //x = a;
            //y = a;
            //y = ProjUnitDiag(x, 2);
            //Log.Message("a = " + String.Join(", ", a));
            //Log.Message("x = " + String.Join(", ", x));
            //Log.Message("y = " + String.Join(", ", y));

            Log.Message("Initialize parentModifierMatrix");
            parentModifierMatrix = IdentityMatrix(order);
            int index = 0;
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
                    parentModifierMatrix[i + k * order] += modifier;
                    parentModifierMatrix[k + i * order] += modifier;
                }
            }

            //Log.Message("Get Big Five Vectors");
            for (int bf = 0; bf < 5; bf++)
            {
                float norm = 0f;
                float[] vector = new float[order];
                foreach (PersonalityNodeDef def in defList)
                {
                    int n = indexDict[def];
                    float modifier = def.bigFiveModifiers[bf];
                    vector[n] = modifier;
                    norm += modifier * modifier;
                }
                norm = Mathf.Sqrt(norm);
                for (int n = 0; n < order; n++)
                {
                    vector[n] /= norm;
                }
                bigFiveVectors.Add(vector);
                //Log.Message("Big five vector " + bf + ": " + string.Join(", ", vector));
            }

            Log.Message("Get weights of Ten Aspects");
            List<float[]> tenAspectsVectors = new List<float[]>();
            List<float[]> tenAspectsWeights = new List<float[]>();
            for (int t = 0; t < 10; t++)
            {
                float norm = 0f;
                float[] vector = new float[order];
                foreach (PersonalityNodeDef def in defList)
                {
                    float weight = def.TenAspects[t];
                    vector[indexDict[def]] = weight;
                    norm += weight * weight;
                }
                tenAspectsWeights.Add(vector);
                float[] vector2 = new float[order];
                norm = Mathf.Sqrt(norm);
                for (int n = 0; n < order; n++)
                {
                    vector2[n] = vector[n] / norm;
                }
                tenAspectsVectors.Add(vector2);
            }
            //float[] cBigFive0 = new float[size];
            float[] cTen0 = new float[size];
            for (int s = 0; s < size; s++)
            {
                (int i, int j) = Get2Dindicies(s);
                //cBigFive0[s] = 0f;
                //for (int bf = 0; bf < 5; bf++)
                //{
                //    cBigFive0[s] += 0.2f * order * bigFiveVectors[bf][i] * bigFiveVectors[bf][j];
                //}
                for (int t = 0; t < 10; t += 2)
                {
                    //cTen0[s] += tenAspectsVectors[t][i] * tenAspectsVectors[t][j];
                    //cTen0[s] += tenAspectsVectors[t + 1][i] * tenAspectsVectors[t + 1][j];
                    //cTen0[s] += 0.33f * tenAspectsVectors[t][i] * tenAspectsVectors[t + 1][j];
                    //cTen0[s] += 0.33f * tenAspectsVectors[t + 1][i] * tenAspectsVectors[t][j];
                    cTen0[s] += tenAspectsWeights[t][i] * tenAspectsWeights[t][j];
                    cTen0[s] += tenAspectsWeights[t + 1][i] * tenAspectsWeights[t + 1][j];
                    cTen0[s] += 0.2f * tenAspectsWeights[t][i] * tenAspectsWeights[t + 1][j];
                    cTen0[s] += 0.2f * tenAspectsWeights[t + 1][i] * tenAspectsWeights[t][j];
                }
                cTen0[s] *= 0.95f;
                cTen0[s] += (s % (order + 1) == 0) ? 0.05f : 0f;
            }

            //Log.Message("Get nearest correlation matrix");
            //float[] cParent0 = NearCorr(parentModifierMatrix);
            //Log.Message("Calculate Big Five correlation matrix");
            //float[] cBigFive1 = NearCorr(cBigFive0);
            //Log.Message("Calculate Ten Aspects correlation matrix");
            //float[] cTen1 = NearCorr(cTen0);

            //Log.Message("Construct ultimate mixture matrix");
            float[] cMix = new float[size];
            for (int s = 0; s < size; s++)
            {
                //List<float> list = new List<float>() { parentModifierMatrix[s], cParent0[s], 0f * cBigFive0[s] + 1f * cTen0[s], 0f * cBigFive1[s] + 1f * cTen1[s] };
                //cMix[s] = list.OrderBy(x => -Mathf.Abs(x)).First();
                cMix[s] = Mathf.Abs(parentModifierMatrix[s]) > 0.01f ? parentModifierMatrix[s] : cTen0[s];

                //Log.Message("list = " + string.Join(", ", list));
                //Log.Message("max = " + cMix[s]);
            }
            //(float[] vMix, float[] dMix) = EigenDecomp(cMix, order);
            //float[] cMixTest = VtimesDtimesTransposeOfV(vMix, dMix);
            //Log.Message("Test VtimesDtimesTransposeOfV: " + MatrixNorm(MatrixDiff(cMix, cMixTest)));


            Log.Message("Calculate ultimate correlation matrix");
            float[] C = NearCorr(cMix);
            for (int s = 0; s < size; s++)
            {
                C[s] *= 0.95f;
                C[s] += (s % (order + 1) == 0) ? 0.05f : 0f;
            }
            Log.Message("Start Decomposition");
            (float[] V, float[] d) = EigenDecomp(C, order);
            //for (int i = 0; i < order; i++)
            //{
            //    d[i] = 0.85f * d[i] + 0.15f;
            //}
            //C = VtimesDtimesTransposeOfV(V, d);
            //C = NearCorr(C);
            //(V, d) = EigenDecomp(C, order);
            //(float[] vParent, float[] dParent) = EigenDecomp(parentModifierMatrix, order);
            (float[] vTen0, float[] dten0) = EigenDecomp(cTen0, order);
            //(float[] vTen1, float[] dten1) = EigenDecomp(cTen1, order);
            //Log.Message("Eigenvalues of parentModifierMatrix = " + string.Join(", ", dParent.Reverse()));
            Log.Message("Eigenvalues of cTen0 = " + string.Join(", ", dten0.Reverse()));
            //Log.Message("Eigenvalues of cTen1 = " + string.Join(", ", dten1.Reverse()));
            Log.Message("Eigenvalues of C = " + string.Join(", ", d.Reverse()));

            //string bigFiveVariances = "Big five variances = ";
            for (int bf = 0; bf < 5; bf++)
            {
                float variance = DotProduct(bigFiveVectors[bf], MatrixVectorProduct(C, bigFiveVectors[bf]));
                bigFiveStandardDevInvs[bf] = 1f / Mathf.Sqrt(variance);
                //bigFiveVariances += variance + ", ";
            }
            //Log.Message(bigFiveVariances);

            float[] dSqrt = new float[order];
            for (int i = 0; i < order; ++i)
            {
                dSqrt[i] = Mathf.Sqrt(Mathf.Abs(d[i]));
            }
            parentTransformMatrix = VtimesDtimesTransposeOfV(V, dSqrt);
            Log.Message("PersonalityNodeParentMatrix initialized");

            foreach (PersonalityNodeDef def in defList)
            {
                int i = indexDict[def];
                string rowOfMCP = def.defName + ": ";
                foreach (PersonalityNodeDef def2 in defList)
                {
                    int j = indexDict[def2];
                    float num1 = cMix[i + j * order];
                    float num2 = C[i + j * order];
                    float num3 = parentTransformMatrix[i + j * order];
                    rowOfMCP += "{" + def2.defName + " " + num1 + ", " + num2 + ", " + num3 + "}, ";
                }
                Log.Message(rowOfMCP);
            }
            for (int j = 0; j < 10; j++)
            {
                List<Pair<string, float>> vecList = new List<Pair<string, float>>();
                foreach (PersonalityNodeDef def in defList)
                {
                    int i = indexDict[def];
                    vecList.Add(new Pair<string, float>(def.defName, V[i + (order - 1 - j) * order]));
                }
                vecList = vecList.OrderBy(pair => -Mathf.Abs(pair.Second)).ToList();
                Log.Message("Eigenvector " + j + ": " + string.Join(", ", vecList));
            }
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            Log.Message("Run time in seconds = " + String.Format("{0:00}.{1:000}", ts.Seconds, ts.Milliseconds));
        }

        public static (int, int) Get2Dindicies(int s)
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

        public static float[] VtimesDtimesTransposeOfV(float[] V, float[] d)
        {
            float[] matrix = new float[size];
            for (int s = 0; s < size; ++s)
            {
                (int i, int j) = Get2Dindicies(s);
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
            for (int s = 0; s < order * order; ++s)
            {
                (int i, int j) = Get2Dindicies(s);
                identity[s] = i == j ? 1f : 0f;
            }
            return identity;
        }

        // --------------------------------------------------

        public static float[] MatrixProduct(float[] A, float[] B)
        {
            float[] C = new float[size];
            for (int s = 0; s < size; ++s)
            {
                //(int i, int j) = Get2Dindicies(s, order);
                int j = s % order;
                float num = 0f;
                for (int k = 0; k < order; ++k)
                {
                    num += A[s - j + k] * B[order * k + j];
                }
                C[s] = num;
            }
            return C;
        }

        // --------------------------------------------------

        // result of multiplying an order x order matrix by a order x 1 column vector (yielding an order x 1 column vector)
        public static float[] MatrixVectorProduct(float[] A, float[] b)
        {
            float[] c = new float[order];
            for (int s = 0; s < size; ++s)
            {
                c[s % order] += A[s] * b[s / order];
            }
            return c;
        }

        public static float DotProduct(float[] x, float[] y)
        {
            float sum = 0f;
            for (int i = 0; i < order; i++)
            {
                sum += x[i] * y[i];
            }
            return sum;
        }

        public static float[] NearCorr(float[] A)
        {
            float tol = 1e-7f;
            int maxits = 100;
            float[] X = new float[size];
            Array.Copy(A, X, size);
            float[] Y = new float[size];
            Array.Copy(A, Y, size);
            int iter = 0;
            float relDiffX = 1e+9f;
            float relDiffY = 1e+9f;
            float relDiffXY = 1e+9f;
            float[] dS = new float[size];
            for (int i = 0; i < size; i++)
            {
                dS[i] = 0f;
            }
            float[] Xold = new float[size];
            float[] Yold = new float[size];
            float[] R;
            while ((relDiffX > tol || relDiffY > tol || relDiffXY > tol) && iter < maxits)
            {
                Array.Copy(X, Xold, size);
                Array.Copy(Y, Yold, size);
                R = MatrixDiff(Y, dS);
                X = ProjSpdEigs(R);
                dS = MatrixDiff(X, R);
                Y = ProjUnitDiag(X);
                relDiffX = MatrixNorm(MatrixDiff(X, Xold)) / MatrixNorm(X);
                relDiffY = MatrixNorm(MatrixDiff(Y, Yold)) / MatrixNorm(Y);
                relDiffXY = MatrixNorm(MatrixDiff(Y, X)) / MatrixNorm(Y);
                iter++;
            }
            Log.Message("Number of iterations for NearCorr = " + iter + ", distance = " + MatrixNorm(MatrixDiff(X, A)));
            return X;
        }

        public static float[] MatrixDiff(float[] A, float[] B)
        {
            float[] C = new float[size];
            for (int i = 0; i < size; i++)
            {
                C[i] = A[i] - B[i];
            }
            return C;
        }

        public static float[] ProjSpdEigs(float[] A)
        {
            (float[] V, float[] d) = EigenDecomp(A, order);
            float dMax = d.OrderBy(x => -Mathf.Abs(x)).First();
            //List<int> indexList = new List<int>();
            //for (int i = 0; i < order; i++)
            //{
            //    if (d[i] > 1e-7f * dMax)
            //    {
            //        indexList.Add(i);
            //    }
            //}
            //float[] B = new float[size];
            //for (int s = 0; s < size; s++)
            //{
            //    B[s] = 0f;
            //    (int i, int j) = Get2Dindicies(s);
            //    foreach (int k in indexList)
            //    {
            //        B[s] += V[i + k * order] * d[k] * V[j + k * order];
            //    }
            //}
            for (int i = 0; i < order; i++)
            {
                if (d[i] < 1e-7f * dMax)
                {
                    d[i] = 0f;
                }
            }
            float[] B = VtimesDtimesTransposeOfV(V, d);
            return B;
        }

        public static float[] ProjUnitDiag(float[] matrix)
        {
            float[] matrix2 = new float[size];
            Array.Copy(matrix, matrix2, size);
            for (int s = 0; s < size; s += order + 1)
            {
                matrix2[s] = 1f;
            }
            return matrix2;
        }

        public static float MatrixNorm(float[] A)
        {
            float norm = 0f;
            for (int i = 0; i < size; ++i)
            {
                norm += A[i] * A[i];
            }
            //return Mathf.Sqrt(norm);
            return norm;
        }

        public static float[] ApplyUpbringingProjection(float[] ratingList, int upbringing)
        {
            float[] x = new float[order];
            for (int i = 0; i < order; i++)
            {
                x[i] = PsycheHelper.NormalCDFInv(ratingList[i]);
            }
            int[] eta = PsycheHelper.GetBitArray(upbringing, 5);
            float[] FinvGFVxMinusVx = new float[5];
            for (int bf = 0; bf < 5; bf++)
            {
                float Vx = DotProduct(bigFiveVectors[bf], x);
                float FVx = PsycheHelper.NormalCDF(Vx);
                float GFVx = 0.5f * (eta[bf] + FVx);
                float FinvGFVx = PsycheHelper.NormalCDFInv(GFVx);
                FinvGFVxMinusVx[bf] = FinvGFVx - Vx;
            }
            float[] newRatings = new float[order];
            for (int i = 0; i < order; i++)
            {
                float y = x[i];
                for (int bf = 0; bf < 5; bf++)
                {
                    y += bigFiveVectors[bf][i] * FinvGFVxMinusVx[bf];
                }
                newRatings[i] = PsycheHelper.NormalCDF(y);
            }
            return newRatings;
        }

    }
}
