using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;
using System.Diagnostics;
using HarmonyLib;
using System.Security.Cryptography;

namespace Psychology;

[StaticConstructorOnStartup]
public static class PersonalityNodeMatrix
{

  public static Dictionary<PersonalityNodeDef, int> indexDict;
  public static Dictionary<int, float[]> PartialProjectionMatrixDict;
  public static float[] parentModifierMatrix;
  public static float[] parentTransformMatrix;
  //public static float[] inverseTransformMatrix;
  public static int order;
  public static int size;
  public static List<float[]> bigFiveVectors = new List<float[]>();
  public static float[] bigFiveStandardDevInvs = new float[5];
  public static double[,] bigFiveMatrix;
  public static double[,] bigFiveInverse;

  public static List<PersonalityNodeDef> DefList => DefDatabase<PersonalityNodeDef>.AllDefsListForReading;

  static PersonalityNodeMatrix()
  {
    //Stopwatch stopwatch = new Stopwatch();
    //stopwatch.Start();

    //Log.Message("Initializing PersonalityNodeParentMatrix");
    //defList = DefDatabase<PersonalityNodeDef>.AllDefsListForReading;

    //Log.Message("Initializing indexDict");
    indexDict = new Dictionary<PersonalityNodeDef, int>();

    //Log.Message("Define order and size");
    order = DefList.Count();
    size = order * order;

    //Log.Message("Initialize parentModifierMatrix");
    parentModifierMatrix = IdentityMatrix(order);
    for (int i = 0; i < DefList.Count(); i++)
    {
      indexDict.Add(DefList[i], i);
    }

    //Log.Message("Adding elements to parentModifierMatrix");
    foreach (PersonalityNodeDef def in DefList)
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
      foreach (PersonalityNodeDef def in DefList)
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
    }

    //Log.Message("Get weights of Ten Aspects");
    List<float[]> tenAspectsVectors = new List<float[]>();
    List<float[]> tenAspectsWeights = new List<float[]>();
    for (int t = 0; t < 10; t++)
    {
      float norm = 0f;
      float[] vector = new float[order];
      foreach (PersonalityNodeDef def in DefList)
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
        cTen0[s] += tenAspectsWeights[t][i] * tenAspectsWeights[t][j];
        cTen0[s] += tenAspectsWeights[t + 1][i] * tenAspectsWeights[t + 1][j];
        cTen0[s] += 0.5f * tenAspectsWeights[t][i] * tenAspectsWeights[t + 1][j];
        cTen0[s] += 0.5f * tenAspectsWeights[t + 1][i] * tenAspectsWeights[t][j];
        //cTen0[s] += 0.2f * tenAspectsVectors[t][i] * tenAspectsVectors[t + 1][j];
        //cTen0[s] += 0.2f * tenAspectsVectors[t + 1][i] * tenAspectsVectors[t][j];
      }
      cTen0[s] *= 0.85f;
      cTen0[s] += (s % (order + 1) == 0) ? 0.15f : 0f;
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


    //Stopwatch stopwatch2 = new Stopwatch();
    //Log.Message("Calculate personality correlation matrix");
    //stopwatch2.Start();
    float[] C = NearCorr(cMix);
    //stopwatch2.Stop();
    //TimeSpan ts2 = stopwatch.Elapsed;
    //Log.Message("NearCorr time in seconds = " + String.Format("{0:00}.{1:000}", ts2.Seconds, ts2.Milliseconds));

    /* ToDo: make this a setting, total correlation */
    for (int s = 0; s < size; s++)
    {
      C[s] *= 0.85f;
      C[s] += (s % (order + 1) == 0) ? 0.15f : 0f;
    }
    //Log.Message("Start Decomposition");
    (float[] V, float[] d) = EigenDecomp(C, order);
    //for (int i = 0; i < order; i++)
    //{
    //    d[i] = 0.85f * d[i] + 0.15f;
    //}
    //C = VtimesDtimesTransposeOfV(V, d);
    //C = NearCorr(C);
    //(V, d) = EigenDecomp(C, order);
    //(float[] vParent, float[] dParent) = EigenDecomp(parentModifierMatrix, order);
    //(float[] vTen0, float[] dten0) = EigenDecomp(cTen0, order);
    //(float[] vTen1, float[] dten1) = EigenDecomp(cTen1, order);
    //Log.Message("Eigenvalues of parentModifierMatrix = " + string.Join(", ", dParent.Reverse()));
    //Log.Message("Eigenvalues of cTen0 = " + string.Join(", ", dten0.Reverse()));
    //Log.Message("Eigenvalues of cTen1 = " + string.Join(", ", dten1.Reverse()));
    //Log.Message("Eigenvalues of C = " + string.Join(", ", d.Reverse()));

    //string bigFiveVariances = "Big five variances = ";
    for (int bf = 0; bf < 5; bf++)
    {
      float variance = DotProduct(bigFiveVectors[bf], MatrixVectorProduct(C, bigFiveVectors[bf]));
      bigFiveStandardDevInvs[bf] = 1f / Mathf.Sqrt(variance);
      //bigFiveVariances += variance + ", ";
    }
    //Log.Message(bigFiveVariances);

    float[] dSqrt = new float[order];
    float[] dInvSqrt = new float[order];
    for (int i = 0; i < order; ++i)
    {
      dSqrt[i] = Mathf.Sqrt(Mathf.Abs(d[i]));
      dInvSqrt[i] = 1f / Mathf.Max(dSqrt[i], 0.001f);
    }
    parentTransformMatrix = VtimesDtimesTransposeOfV(V, dSqrt);
    //inverseTransformMatrix = VtimesDtimesTransposeOfV(V, dInvSqrt);

    //Log.Message("PersonalityNodeParentMatrix initialized");

    //foreach (PersonalityNodeDef def in defList)
    //{
    //    int i = indexDict[def];
    //    string rowOfMCP = def.defName + ": ";
    //    foreach (PersonalityNodeDef def2 in defList)
    //    {
    //        int j = indexDict[def2];
    //        float num1 = cMix[i + j * order];
    //        float num2 = C[i + j * order];
    //        float num3 = parentTransformMatrix[i + j * order];
    //        rowOfMCP += "{" + def2.defName + " " + num1 + ", " + num2 + ", " + num3 + "}, ";
    //    }
    //    //Log.Message(rowOfMCP);
    //}
    //for (int j = 0; j < 10; j++)
    //{
    //    List<Pair<string, float>> vecList = new List<Pair<string, float>>();
    //    foreach (PersonalityNodeDef def in defList)
    //    {
    //        int i = indexDict[def];
    //        vecList.Add(new Pair<string, float>(def.defName, V[i + (order - 1 - j) * order]));
    //    }
    //    vecList = vecList.OrderBy(pair => -Mathf.Abs(pair.Second)).ToList();
    //    //Log.Message("Eigenvector " + j + ": " + string.Join(", ", vecList));
    //}

    //stopwatch.Stop();
    //TimeSpan ts = stopwatch.Elapsed;
    //Log.Message("Calculation time in seconds = " + String.Format("{0:00}.{1:000}", ts.Seconds, ts.Milliseconds));
    Log.Message("Psychology: calculated personality correlation matrix");

    PsycheHelper.InitializeDictionariesForPersonalityNodeDefs();

    bigFiveMatrix = new double[5, order];
    for (int i = 0; i < order; i++)
    {
      for (int bf = 0; bf < 5; bf++)
      {
        bigFiveMatrix[bf, i] = 0.5f * PersonalityNodeMatrix.bigFiveStandardDevInvs[bf] * bigFiveVectors[bf][i];
      }
    }

    // bigFiveInverse will be an order by 5 matrix
    bigFiveInverse = PInv(bigFiveMatrix);
    Log.Message("bigFiveMatrix rows: " + bigFiveMatrix.GetLength(0) + ", columns: " + bigFiveMatrix.GetLength(1));
    Log.Message("bigFiveInverse rows: " + bigFiveInverse.GetLength(0) + ", columns: " + bigFiveInverse.GetLength(1));
  }


  public static (int, int) Get2Dindicies(int s) => (s % order, s / order);

  /// <summary>
  /// Computes the eigenvalues and eigenvectors of a matrix.
  /// </summary>
  /// <param name="order">The order of the matrix.</param>
  /// <param name="matrix">The matrix to decompose. The length of the array must be order * order.</param>
  // public void EigenDecomp(int order, float[] matrix, float[] matrixEv, Complex[] vectorEv, float[] matrixD)
  public static (float[], float[]) EigenDecomp(float[] matrix, int order)
  {
    if (matrix.Length != order * order)
    {
      Log.Error("Psychology.EigenDecomp: matrix.Length: " + matrix.Length + ", order^2: " + order * order);
    }

    float[] matrixV = new float[order * order];
    float[] d = new float[order];
    float[] e = new float[order];

    Buffer.BlockCopy(matrix, 0, matrixV, 0, matrix.Length * sizeof(float));
    int om1 = order - 1;
    for (int i = 0; i < order; ++i)
    {
      d[i] = matrixV[i * order + om1];
    }

    SymmetricTridiagonalize(matrixV, d, e, order);
    SymmetricDiagonalize(matrixV, d, e, order);

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
    return (matrixV, d);
  }

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
  public static void SymmetricTridiagonalize(float[] a, float[] d, float[] e, int order)
  {
    //Log.Message("SymmetricTridiagonalize");
    // Householder reduction to tridiagonal form.
    if (a.Length != order * order || d.Length != order || e.Length != order || order == 0)
    {
      Log.Error("Psychology.SymmetricTridiagonalize: a,d,e,order: " + String.Join(",", new int[] { a.Length, d.Length, e.Length, order }));
    }

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
  public static void SymmetricDiagonalize(float[] a, float[] d, float[] e, int order)
  {
    if (a.Length != order * order || d.Length != order || e.Length != order || order == 0)
    {
      Log.Error("Psychology.SymmetricDiagonalize: a, d, e, order: " + String.Join(",", new int[] { a.Length, d.Length, e.Length, order }));
    }

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
          if (order - 1 <= l)
          {
            Log.Error("PersonalityNodeMatrix.SymmetricDiagonalize: order - 1 <= l: order = " + order + ", l = " + l + ", m = " + m);
            break;
          }

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
            Log.Error("PersonalityNodeMatrix.SymmetricDiagonalize: Convergence failed");
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
    if (b != 0.0f)
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

  //public static float[] ZeroArray(int order)
  //{
  //    float[] zeroArray = new float[order];
  //    for (int i = 0; i < order; ++i)
  //    {
  //        zeroArray[i] = 0f;
  //    }
  //    return zeroArray;
  //}

  public static float[] IdentityMatrix(int order)
  {
    // return an order x order Identity matrix
    //float[] identity = new float[order * order];
    //int s;
    //int i;
    //int j;
    //for (s = 0; s < order * order; ++s)
    //{
    //    (i, j) = Get2Dindicies(s);
    //    identity[s] = i == j ? 1f : 0f;
    //}
    //return identity;

    // return an order x order Identity matrix
    float[] identity = new float[order * order];
    int s;
    for (s = 0; s < order * order; s += order + 1)
    {
      identity[s] = 1f;
    }
    return identity;
  }

  // --------------------------------------------------

  public static float[] MatrixProduct(float[] A, float[] B)
  {
    float[] C = new float[size];
    int j;
    float num;
    for (int s = 0; s < size; ++s)
    {
      //(int i, int j) = Get2Dindicies(s, order);
      j = s % order;
      num = 0f;
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
      relDiffX = MatrixNormSquared(MatrixDiff(X, Xold)) / MatrixNormSquared(X);
      relDiffY = MatrixNormSquared(MatrixDiff(Y, Yold)) / MatrixNormSquared(Y);
      relDiffXY = MatrixNormSquared(MatrixDiff(Y, X)) / MatrixNormSquared(Y);
      iter++;
    }
    //Log.Message("Number of iterations for NearCorr = " + iter + ", distance = " + MatrixNorm(MatrixDiff(X, A)));
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

  public static float MatrixNormSquared(float[] A)
  {
    float norm = 0f;
    for (int i = 0; i < size; ++i)
    {
      norm += A[i] * A[i];
    }
    return Mathf.Sqrt(norm);
    //return norm;
  }


  public static void ApplyBigFiveProjections(float[] ratingList)
  {
    if (PsychologySettings.personalityExtremeness == 0f)
    {
      return;
    }
    float[] x = new float[order];
    for (int i = 0; i < order; i++)
    {
      // Map from uniformly random from 0 to 1 to normally distributed random
      x[i] = PsycheHelper.NormalCDFInv(ratingList[i]);
    }
    float[] FinvGFVxMinusVx = new float[5];
    for (int bf = 0; bf < 5; bf++)
    {
      // Project onto the Big Five
      float Vx = DotProduct(bigFiveVectors[bf], x);
      // Map back to uniformly random in the range [0,1]
      float FVx = PsycheHelper.NormalCDF(Vx);
      // Restrict to either lower or upper interval based sign of FVx - 0.5, thus making personalities more extreme
      float GFVx = (1f - PsychologySettings.personalityExtremeness) * FVx + (FVx < 0.5f ? 0f : PsychologySettings.personalityExtremeness);
      // Map back to normally distributed random
      float FinvGFVx = PsycheHelper.NormalCDFInv(GFVx);
      // Subtract original projection
      FinvGFVxMinusVx[bf] = FinvGFVx - Vx;
    }
    for (int i = 0; i < order; i++)
    {
      // Start with y = x
      float y = x[i];
      for (int bf = 0; bf < 5; bf++)
      {
        // Add net projection from big five, y = x + V Vx
        y += bigFiveVectors[bf][i] * FinvGFVxMinusVx[bf];
      }
      // Map back to uniformly random in the range [0,1]
      ratingList[i] = PsycheHelper.NormalCDF(y);
    }

  }

  /// <summary>
  ///     Return the pseudoinverse of a matrix based on the Moore-Penrose Algorithm.
  ///     using Singular Value Decomposition (SVD).
  /// </summary>
  /// <param name="inMat">Input matrix to find its inverse to.</param>
  /// <returns>The inverse matrix approximation of the input matrix.</returns>
  public static double[,] PInv(double[,] inMat)
  {
    // To compute the SVD of the matrix to find Sigma.
    var (u, s, v) = Decompose(inMat);

    // To take the reciprocal of each non-zero element on the diagonal.
    var len = s.Length;

    var sigma = new double[len];
    for (var i = 0; i < len; i++)
    {
      sigma[i] = Math.Abs(s[i]) < 0.0001 ? 0 : 1 / s[i];
    }

    // To construct a diagonal matrix based on the vector result.
    var diag = sigma.ToDiagonalMatrix();

    // To construct the pseudo-inverse using the computed information above.
    var matinv = u.Multiply(diag).Multiply(v.Transpose());

    // To Transpose the result matrix.
    return matinv.Transpose();
  }

  public static (double[,] U, double[] S, double[,] V) Decompose(double[,] matrix) =>
            Decompose(matrix, 1E-5, 100);

  /// <summary>
  ///     Computes the SVD for the given matrix, with singular values arranged from greatest to least.
  /// </summary>
  /// <param name="matrix">The matrix.</param>
  /// <param name="epsilon">The error margin.</param>
  /// <param name="maxIterations">The maximum number of iterations.</param>
  /// <returns>The SVD.</returns>
  public static (double[,] U, double[] S, double[,] V) Decompose(
      double[,] matrix,
      double epsilon,
      int maxIterations)
  {
    var m = matrix.GetLength(0);
    var n = matrix.GetLength(1);
    var numValues = Math.Min(m, n);

    // sigmas is be a diagonal matrix, hence only a vector is needed
    double[] sigmas = new double[numValues];
    double[,] us = new double[m, numValues];
    double[,] vs = new double[n, numValues];

    // keep track of progress

    double[,] remaining = matrix.MatrixCopy();

    // for each singular value
    for (var i = 0; i < numValues; i++)
    {
      // compute the v singular vector
      double[] v = Decompose1D(remaining, epsilon, maxIterations);
      double[] u = matrix.MultiplyVector(v);

      // compute the contribution of this pair of singular vectors
      double[,] contrib = u.OuterProduct(v);

      // extract the singular value
      var s = u.Magnitude();

      // v and u should be unit vectors
      if (s < epsilon)
      {
        u = new double[m];
        v = new double[n];
      }
      else
      {
        u = u.Scale(1 / s);
      }

      // save u, v and s into the result
      for (var j = 0; j < u.Length; j++)
      {
        us[j, i] = u[j];
      }

      for (var j = 0; j < v.Length; j++)
      {
        vs[j, i] = v[j];
      }

      sigmas[i] = s;

      // remove the contribution of this pair and compute the rest
      remaining = remaining.Subtract(contrib);
    }

    return (U: us, S: sigmas, V: vs);
  }

  /// <summary>
  ///     Generates a diagonal matrix from an specified vector.
  /// </summary>
  /// <param name="vector">The input vector.</param>
  /// <returns>A Diagonal matrix.</returns>
  public static double[,] ToDiagonalMatrix(this double[] vector)
  {
    var len = vector.Length;
    var result = new double[len, len];

    for (var i = 0; i < len; i++)
    {
      result[i, i] = vector[i];
    }

    return result;
  }

  /// <summary>
  ///     Performs immutable dot product multiplication on source matrix to operand.
  /// </summary>
  /// <param name="source">Source left matrix.</param>
  /// <param name="operand">Operand right matrix.</param>
  /// <returns>Dot product result.</returns>
  /// <exception cref="InvalidOperationException">The width of a first operand should match the height of a second.</exception>
  public static double[,] Multiply(this double[,] source, double[,] operand)
  {
    if (source.GetLength(1) != operand.GetLength(0))
    {
      throw new InvalidOperationException(
          "The width of a first operand should match the height of a second.");
    }

    var result = new double[source.GetLength(0), operand.GetLength(1)];

    for (var i = 0; i < result.GetLength(0); i++)
    {
      for (var j = 0; j < result.GetLength(1); j++)
      {
        double elementProduct = 0;

        for (var k = 0; k < source.GetLength(1); k++)
        {
          elementProduct += source[i, k] * operand[k, j];
        }

        result[i, j] = elementProduct;
      }
    }

    return result;
  }

  /// <summary>
  ///     Makes a copy of a matrix. Changes to the copy should not affect the original.
  /// </summary>
  /// <param name="matrix">The matrix.</param>
  /// <returns>A copy of the matrix.</returns>
  public static double[,] MatrixCopy(this double[,] matrix)
  {
    var result = new double[matrix.GetLength(0), matrix.GetLength(1)];
    for (var i = 0; i < matrix.GetLength(0); i++)
    {
      for (var j = 0; j < matrix.GetLength(1); j++)
      {
        result[i, j] = matrix[i, j];
      }
    }

    return result;
  }

  /// <summary>
  ///     Transposes a matrix.
  /// </summary>
  /// <param name="matrix">The matrix.</param>
  /// <returns>The transposed matrix.</returns>
  public static double[,] Transpose(this double[,] matrix)
  {
    var result = new double[matrix.GetLength(1), matrix.GetLength(0)];
    for (var i = 0; i < matrix.GetLength(0); i++)
    {
      for (var j = 0; j < matrix.GetLength(1); j++)
      {
        result[j, i] = matrix[i, j];
      }
    }

    return result;
  }

  /// <summary>
  ///     Performs matrix subtraction.
  /// </summary>
  /// <param name="lhs">The LHS matrix.</param>
  /// <param name="rhs">The RHS matrix.</param>
  /// <returns>The difference of the two matrices.</returns>
  /// <exception cref="ArgumentException">Dimensions of matrices do not match.</exception>
  public static double[,] Subtract(this double[,] lhs, double[,] rhs)
  {
    if (lhs.GetLength(0) != rhs.GetLength(0) ||
        lhs.GetLength(1) != rhs.GetLength(1))
    {
      throw new ArgumentException("Dimensions of matrices must be the same");
    }

    var result = new double[lhs.GetLength(0), lhs.GetLength(1)];
    for (var i = 0; i < lhs.GetLength(0); i++)
    {
      for (var j = 0; j < lhs.GetLength(1); j++)
      {
        result[i, j] = lhs[i, j] - rhs[i, j];
      }
    }

    return result;
  }

  /// <summary>
  ///     Computes a single singular vector for the given matrix, corresponding to the largest singular value.
  /// </summary>
  /// <param name="matrix">The matrix.</param>
  /// <returns>A singular vector, with dimension equal to number of columns of the matrix.</returns>
  public static double[] Decompose1D(double[,] matrix) =>
      Decompose1D(matrix, 1E-5, 100);

  /// <summary>
  ///     Computes a single singular vector for the given matrix, corresponding to the largest singular value.
  /// </summary>
  /// <param name="matrix">The matrix.</param>
  /// <param name="epsilon">The error margin.</param>
  /// <param name="maxIterations">The maximum number of iterations.</param>
  /// <returns>A singular vector, with dimension equal to number of columns of the matrix.</returns>
  public static double[] Decompose1D(double[,] matrix, double epsilon, int maxIterations)
  {
    var n = matrix.GetLength(1);
    var iterations = 0;
    double mag;
    double[] currIteration = RandomUnitVector(n);
    double[] lastIteration = new double[currIteration.Length];
    double[,] b = matrix.Transpose().Multiply(matrix);
    do
    {
      Array.Copy(currIteration, lastIteration, currIteration.Length);
      currIteration = b.MultiplyVector(lastIteration);
      currIteration = currIteration.Scale(100);
      mag = currIteration.Magnitude();
      if (mag > epsilon)
      {
        currIteration = currIteration.Scale(1 / mag);
      }

      iterations++;
    }
    while (lastIteration.Dot(currIteration) < 1 - epsilon && iterations < maxIterations);

    return currIteration;
  }

  /// <summary>
  ///     Multiplies a matrix by a vector.
  /// </summary>
  /// <param name="matrix">The matrix.</param>
  /// <param name="vector">The vector.</param>
  /// <returns>The product of the matrix and the vector, which is a vector.</returns>
  /// <exception cref="ArgumentException">Dimensions of matrix and vector do not match.</exception>
  public static double[] MultiplyVector(this double[,] matrix, double[] vector)
  {
    var vectorReshaped = new double[vector.Length, 1];
    for (var i = 0; i < vector.Length; i++)
    {
      vectorReshaped[i, 0] = vector[i];
    }

    var resultMatrix = matrix.Multiply(vectorReshaped);
    var result = new double[resultMatrix.GetLength(0)];
    for (var i = 0; i < result.Length; i++)
    {
      result[i] = resultMatrix[i, 0];
    }

    return result;
  }

  /// <summary>
  ///     Computes a random unit vector.
  /// </summary>
  /// <param name="dimensions">The dimensions of the required vector.</param>
  /// <returns>The unit vector.</returns>
  public static double[] RandomUnitVector(int dimensions)
  {
    System.Random random = new();
    double[] result = new double[dimensions];
    for (var i = 0; i < dimensions; i++)
    {
      result[i] = 2 * random.NextDouble() - 1;
    }

    var magnitude = result.Magnitude();
    result = result.Scale(1 / magnitude);
    return result;
  }

  /// <summary>
  ///     Returns the scaled vector.
  /// </summary>
  /// <param name="vector">The vector.</param>
  /// <param name="factor">Scale factor.</param>
  /// <returns>The unit vector.</returns>
  public static double[] Scale(this double[] vector, double factor)
  {
    var result = new double[vector.Length];
    for (var i = 0; i < vector.Length; i++)
    {
      result[i] = vector[i] * factor;
    }

    return result;
  }

  /// <summary>
  ///     Computes the magnitude of a vector.
  /// </summary>
  /// <param name="vector">The vector.</param>
  /// <returns>The magnitude.</returns>
  public static double Magnitude(this double[] vector)
  {
    var magnitude = Dot(vector, vector);
    magnitude = Math.Sqrt(magnitude);
    return magnitude;
  }

  /// <summary>
  ///     Computes the dot product of two vectors.
  /// </summary>
  /// <param name="lhs">The LHS vector.</param>
  /// <param name="rhs">The RHS vector.</param>
  /// <returns>The dot product of the two vector.</returns>
  /// <exception cref="ArgumentException">Dimensions of vectors do not match.</exception>
  public static double Dot(this double[] lhs, double[] rhs)
  {
    if (lhs.Length != rhs.Length)
    {
      throw new ArgumentException("Dot product arguments must have same dimension");
    }

    double result = 0;
    for (var i = 0; i < lhs.Length; i++)
    {
      result += lhs[i] * rhs[i];
    }

    return result;
  }

  /// <summary>
  ///     Computes the outer product of two vectors.
  /// </summary>
  /// <param name="lhs">The LHS vector.</param>
  /// <param name="rhs">The RHS vector.</param>
  /// <returns>The outer product of the two vector.</returns>
  public static double[,] OuterProduct(this double[] lhs, double[] rhs)
  {
    var result = new double[lhs.Length, rhs.Length];
    for (var i = 0; i < lhs.Length; i++)
    {
      for (var j = 0; j < rhs.Length; j++)
      {
        result[i, j] = lhs[i] * rhs[j];
      }
    }

    return result;
  }

  public static float[] ToFloatArray(this double[] lhs)
  {
    float[] result = new float[lhs.Length];
    for (int i = 0; i < lhs.Length; i++)
    {
      result[i] = (float)lhs[i];
    }
    return result;
  }

  public static float[,] ToFloatArray(this double[,] lhs)
  {
    float[,] result = new float[lhs.GetLength(0), lhs.GetLength(1)];
    for (int i = 0; i < lhs.GetLength(0); i++)
    {
      for (int j = 0; i < lhs.GetLength(1); j++)
      {
        result[i, j] = (float)lhs[i, j];
      }
    }
    return result;
  }




}
