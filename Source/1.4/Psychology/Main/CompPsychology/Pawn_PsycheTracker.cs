using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using UnityEngine;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements.Experimental;
using System.Reflection;

namespace Psychology;

public class Pawn_PsycheTracker : IExposable
{
  public int upbringing = 0;
  public int lastDateTick = 0;
  private Pawn pawn;
  private HashSet<PersonalityNode> nodes;

  public Dictionary<PersonalityNodeDef, PersonalityNode> nodeDict = new Dictionary<PersonalityNodeDef, PersonalityNode>();
  private Dictionary<string, float> cachedOpinions = new Dictionary<string, float>();
  private Dictionary<string, bool> recalcCachedOpinions = new Dictionary<string, bool>();

  public Dictionary<Pair<string, string>, float> cachedDisagreementWeights = new Dictionary<Pair<string, string>, float>();
  public Dictionary<Pair<string, string>, bool> recalcNodeDisagreement = new Dictionary<Pair<string, string>, bool>();
  public int AdjustedRatingTicker = -1;
  public Dictionary<MemeDef, Dictionary<PersonalityNodeDef, float>> dailyCertaintyFromMemesAndNodes = new Dictionary<MemeDef, Dictionary<PersonalityNodeDef, float>>();
  public Dictionary<PreceptDef, Dictionary<PersonalityNodeDef, float>> dailyCertaintyFromPerceptsAndNodes = new Dictionary<PreceptDef, Dictionary<PersonalityNodeDef, float>>();

  //public float[] RawRatingArray = new float[PersonalityNodeMatrix.order];
  //public Dictionary<PersonalityNodeDef, float> RawRatingDict = new Dictionary<PersonalityNodeDef, float>();
  //public Dictionary<PersonalityNodeDef, float> AdjustedRatingDict = new Dictionary<PersonalityNodeDef, float>();

  public HashSet<PersonalityNode> PersonalityNodes => nodes;
  //{
  //  get
  //  {
  //    return nodes;
  //  }
  //  set
  //  {
  //    nodes = value;
  //  }
  //}

  public Dictionary<string, bool> OpinionCacheDirty
  {
    get => recalcCachedOpinions;
    set => recalcCachedOpinions = value;
  }

  public Dictionary<Pair<string, string>, bool> DisagreementCacheDirty
  {
    get => recalcNodeDisagreement;
    set => recalcNodeDisagreement = value;
  }

  public Pawn_PsycheTracker(Pawn p)
  {
    this.pawn = p;
  }

  public void ExposeData()
  {
    Scribe_Values.Look(ref this.upbringing, "upbringing", 0, false);
    Scribe_Values.Look(ref this.lastDateTick, "lastDateTick", 0, false);
    PsycheHelper.Look(ref this.nodes, "nodes", LookMode.Deep, new object[] { this.pawn });
    foreach (PersonalityNode n in this.nodes)
    {
      this.nodeDict[n.def] = n;
    }
  }

  public void Initialize(int inputSeed = 0)
  {
    this.nodes = new HashSet<PersonalityNode>();
    foreach (PersonalityNodeDef def in PersonalityNodeMatrix.DefList)
    {
      this.nodes.Add(PersonalityNodeMaker.MakeNode(def, this.pawn));
    }
    RandomizeRatings(inputSeed);
    foreach (PersonalityNode n in this.nodes)
    {
      this.nodeDict[n.def] = n;
    }
  }

  public void RandomizeRatings(int inputSeed = 0)
  {
    int pawnSeed = PsycheHelper.PawnSeed(this.pawn);
    int defSeed;
    foreach (PersonalityNode node in nodes)
    {
      defSeed = node.def.GetHashCode();
      node.rawRating = Rand.ValueSeeded(Gen.HashCombineInt(pawnSeed, defSeed, inputSeed, 37));
      //RawRatingArray[PersonalityNodeMatrix.indexDict[node.def]] = node.rawRating;
      //RawRatingDict[node.def] = node.rawRating;
    }
    AdjustedRatingTicker = -1;
  }

  public float GetPersonalityRating(PersonalityNodeDef def)
  {
    if (AdjustedRatingTicker < 0)
    {
      CalculateAdjustedRatings();
    }
    AdjustedRatingTicker--;
    return nodeDict[def].AdjustedRating;
    //return AdjustedRatingDict[def];
  }

  public PersonalityNode GetPersonalityNodeOfDef(PersonalityNodeDef def) => nodeDict[def];

  public float GetConversationTopicWeight(PersonalityNodeDef def, Pawn otherPawn)
  {
    /* Pawns will avoid controversial topics until they know someone better.
     * This isn't a perfect system, but the weights will be closer together the higher totalOpinionModifiers is.
     */
    float totalOpinion = this.TotalThoughtOpinion(otherPawn, out IEnumerable<Thought_MemorySocialDynamic> convoMemories);
    float t = Mathf.Max(0f, totalOpinion / 30f + GetPersonalityRating(PersonalityNodeDefOf.Outspoken) - GetPersonalityRating(PersonalityNodeDefOf.Polite));
    float c = def.controversiality;
    float weight = 1f / (Mathf.Pow(32f, c) + 64f * t * t);
    //float weight = 10f / Mathf.Lerp(1f + 8f * def.controversiality, 1f + 0.5f * def.controversiality, t);

    /* Polite pawns will avoid topics they already know are contentious. */
    Pair<string, string> disagreementKey = new Pair<string, string>(otherPawn.ThingID, def.defName);
    if (cachedDisagreementWeights.TryGetValue(disagreementKey, out float cachedWeight) && !recalcNodeDisagreement[disagreementKey])
    {
      weight *= cachedWeight;
    }
    else
    {
      float knownDisagreements = (from m in convoMemories
                                  where m.opinionOffset < 0f && m.CurStage.label == def.defName
                                  select Math.Abs(m.opinionOffset)).Sum();
      //float disagree = 1f;
      //if (knownDisagreements > 0)
      //{
      //    disagree = Mathf.Clamp01(1f / (knownDisagreements / 50f)) * Mathf.Lerp(0.25f, 1f, this.GetPersonalityRating(PersonalityNodeDefOf.Polite));
      //}
      float disagree = Mathf.Lerp(1f, 0.25f, 0.1f * knownDisagreements * GetPersonalityRating(PersonalityNodeDefOf.Polite) - 0.25f);
      cachedDisagreementWeights[disagreementKey] = disagree;
      recalcNodeDisagreement[disagreementKey] = false;
      weight *= disagree;
    }
    return weight;
  }

  public float TotalThoughtOpinion(Pawn other, out IEnumerable<Thought_MemorySocialDynamic> convoMemories)
  {
    convoMemories = from m in this.pawn.needs.mood.thoughts.memories.Memories.OfType<Thought_MemorySocialDynamic>()
                    where m.def.defName.Contains("Conversation") && m.otherPawn == other
                    select m;
    if (cachedOpinions.ContainsKey(other.ThingID) && !recalcCachedOpinions[other.ThingID])
    {
      return cachedOpinions[other.ThingID];
    }
    //float knownThoughtOpinion = 1f;
    //convoMemories.Do(m => knownThoughtOpinion += Math.Abs(m.opinionOffset));
    float knownThoughtOpinion = 0f;
    convoMemories.Do(m => knownThoughtOpinion += m.opinionOffset);

    cachedOpinions[other.ThingID] = knownThoughtOpinion;
    recalcCachedOpinions[other.ThingID] = false;
    return knownThoughtOpinion;
  }

  public void CalculateAdjustedRatings()
  {
    //Stopwatch stopwatch = new Stopwatch();
    //stopwatch.Start();
    Gender gender = pawn.gender;
    //stopwatch.Stop();
    //PsycheHelper.CircumstanceTimings[0] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //stopwatch.Reset();

    //stopwatch.Start();
    List<Trait> traits = pawn.story.traits.allTraits;
    //stopwatch.Stop();
    //PsycheHelper.CircumstanceTimings[1] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //stopwatch.Reset();

    //stopwatch.Start();
    List<SkillRecord> skills = pawn.skills.skills;
    //stopwatch.Stop();
    //PsycheHelper.CircumstanceTimings[2] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //stopwatch.Reset();

    //stopwatch.Start();
    List<WorkTypeDef> incapables = pawn.GetDisabledWorkTypes();
    //stopwatch.Stop();
    //PsycheHelper.CircumstanceTimings[3] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //stopwatch.Reset();

    int index;
    float[] adjustedRatingList = new float[PersonalityNodeMatrix.order];
    foreach (PersonalityNode node in this.nodes)
    {
      adjustedRatingList[PersonalityNodeMatrix.indexDict[node.def]] = node.rawRating;
    }
    AdjustForCircumstance(adjustedRatingList, true, gender, traits, skills, incapables);
    for (index = 0; index < PersonalityNodeMatrix.order; index++)
    {
      adjustedRatingList[index] = PsycheHelper.NormalCDFInv(adjustedRatingList[index]);
    }
    adjustedRatingList = PersonalityNodeMatrix.MatrixVectorProduct(PersonalityNodeMatrix.parentTransformMatrix, adjustedRatingList);
    for (index = 0; index < PersonalityNodeMatrix.order; index++)
    {
      adjustedRatingList[index] = PsycheHelper.NormalCDF(adjustedRatingList[index]);
    }
    AdjustForCircumstance(adjustedRatingList, true, gender, traits, skills, incapables);
    PersonalityNodeMatrix.ApplyBigFiveProjections(adjustedRatingList);
    foreach (PersonalityNode node in this.nodes)
    {
      node.AdjustedRating = adjustedRatingList[PersonalityNodeMatrix.indexDict[node.def]];
    }
    AdjustedRatingTicker = 500;
    //string text = "AdjustForCircumstance, total timings in ms:";
    //for (index = 0; index < PsycheHelper.CircumstanceTimings.Count(); index++)
    //{
    //    text += " | {" + index + ", " + PsycheHelper.CircumstanceTimings[index] + "}";
    //}
    ////Log.Message(text);
    ////Log.Message("AdjustForCircumstance, average timings in ms: " + PsycheHelper.CircumstanceTimings.Sum() / PsycheHelper.CircumstanceCount + " for count = " + PsycheHelper.CircumstanceCount);
  }

  public void AdjustForCircumstance(float[] ratings, bool applyTwice, Gender gender, List<Trait> traits, List<SkillRecord> skills, List<WorkTypeDef> incapables)
  {
    //PsycheHelper.CircumstanceCount++;
    //Stopwatch stopwatch = new Stopwatch();
    //Stopwatch stopwatch2 = new Stopwatch();
    //Stopwatch stopwatch3 = new Stopwatch();

    float[] tM = new float[PersonalityNodeMatrix.order];
    float s;
    float m;
    int index;
    Dictionary<int, float> dict;

    //stopwatch.Start();
    float gM;
    float gMm1;
    float kinseyFactor = PsychologySettings.enableKinsey ? 1f - PsycheHelper.Comp(pawn).Sexuality.kinseyRating / 6f : pawn.story.traits.HasTrait(TraitDefOf.Gay) ? 0f : 1f;
    //stopwatch.Stop();
    //PsycheHelper.CircumstanceTimings[4] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //stopwatch.Reset();

    //stopwatch.Start();
    if (PsycheHelper.GenderModifierNodeDefDict.TryGetValue(gender, out dict) && kinseyFactor != 0f)
    {
      //stopwatch2.Start();
      foreach (KeyValuePair<int, float> kvp in dict)
      {
        //stopwatch3.Start();
        gM = kvp.Value * kinseyFactor;
        if (Mathf.Abs(gM) > 0.001f)
        {
          gMm1 = gM - 1f;
          ratings[kvp.Key] = (gMm1 + Mathf.Sqrt(gMm1 * gMm1 + 4f * gM * ratings[kvp.Key])) / (2f * gM);
        }
        //stopwatch3.Stop();
        //PsycheHelper.CircumstanceTimings[5] += (float)stopwatch3.Elapsed.TotalMilliseconds;
        //PsycheHelper.CircumstanceTimings[6] -= (float)stopwatch3.Elapsed.TotalMilliseconds;
        //stopwatch3.Reset();
      }
      //stopwatch2.Stop();
      //PsycheHelper.CircumstanceTimings[6] += (float)stopwatch2.Elapsed.TotalMilliseconds;
      //PsycheHelper.CircumstanceTimings[7] -= (float)stopwatch2.Elapsed.TotalMilliseconds;
      //stopwatch2.Reset();
    }
    //stopwatch.Stop();
    //PsycheHelper.CircumstanceTimings[7] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //stopwatch.Reset();

    //stopwatch.Start();
    Pair<TraitDef, int> traitPair = new Pair<TraitDef, int>();
    bool trygetvalue;
    if (traits.NullOrEmpty() != true)
    {
      foreach (Trait trait in traits)
      {
        //stopwatch2.Start();
        traitPair = new Pair<TraitDef, int>(trait.def, trait.Degree);
        trygetvalue = PsycheHelper.TraitModifierNodeDefDict.TryGetValue(traitPair, out dict);
        //stopwatch2.Stop();
        //PsycheHelper.CircumstanceTimings[8] += (float)stopwatch2.Elapsed.TotalMilliseconds;
        //PsycheHelper.CircumstanceTimings[10] -= (float)stopwatch2.Elapsed.TotalMilliseconds;
        //stopwatch2.Reset();

        if (trygetvalue != true)
        {
          continue;
        }

        //stopwatch2.Start();
        foreach (KeyValuePair<int, float> kvp in dict)
        {
          index = kvp.Key;
          tM[index] = PsycheHelper.RelativisticAddition(tM[index], kvp.Value);

        }
        //stopwatch2.Stop();
        //PsycheHelper.CircumstanceTimings[9] += (float)stopwatch2.Elapsed.TotalMilliseconds;
        //PsycheHelper.CircumstanceTimings[10] -= (float)stopwatch2.Elapsed.TotalMilliseconds;
        //stopwatch2.Reset();
      }
    }
    //stopwatch.Stop();
    //PsycheHelper.CircumstanceTimings[10] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //stopwatch.Reset();

    //stopwatch.Start();
    float skillWeight;
    if (skills.NullOrEmpty() != true)
    {
      foreach (SkillRecord skill in skills)
      {
        //stopwatch2.Start();
        trygetvalue = PsycheHelper.SkillModifierNodeDefDict.TryGetValue(skill.def, out HashSet<int> indices);
        //stopwatch2.Stop();
        //PsycheHelper.CircumstanceTimings[11] += (float)stopwatch2.Elapsed.TotalMilliseconds;
        //PsycheHelper.CircumstanceTimings[15] -= (float)stopwatch2.Elapsed.TotalMilliseconds;
        //stopwatch2.Reset();

        if (trygetvalue != true)
        {
          continue;
        }

        //stopwatch2.Start();
        foreach (int i in indices)
        {
          //stopwatch3.Start();
          skillWeight = Mathf.Lerp(-0.20f, 0.85f, skill.levelInt / 20f);
          //stopwatch3.Stop();
          //PsycheHelper.CircumstanceTimings[12] += (float)stopwatch3.Elapsed.TotalMilliseconds;
          //PsycheHelper.CircumstanceTimings[14] -= (float)stopwatch3.Elapsed.TotalMilliseconds;
          //stopwatch3.Reset();

          //stopwatch3.Start();
          tM[i] = PsycheHelper.RelativisticAddition(tM[i], skillWeight);
          //stopwatch3.Stop();
          //PsycheHelper.CircumstanceTimings[13] += (float)stopwatch3.Elapsed.TotalMilliseconds;
          //PsycheHelper.CircumstanceTimings[14] -= (float)stopwatch3.Elapsed.TotalMilliseconds;
          //stopwatch3.Reset();
        }
        //stopwatch2.Stop();
        //PsycheHelper.CircumstanceTimings[14] += (float)stopwatch2.Elapsed.TotalMilliseconds;
        //PsycheHelper.CircumstanceTimings[15] -= (float)stopwatch2.Elapsed.TotalMilliseconds;
        //stopwatch2.Reset();

      }
    }
    //stopwatch.Stop();
    //PsycheHelper.CircumstanceTimings[15] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //stopwatch.Reset();

    //stopwatch.Start();
    if (incapables.NullOrEmpty() != true)
    {
      foreach (WorkTypeDef incapable in incapables)
      {
        if (PsycheHelper.IncapableModifierNodeDefDict.TryGetValue(incapable, out dict) != true)
        {
          continue;
        }
        foreach (KeyValuePair<int, float> kvp in dict)
        {
          index = kvp.Key;
          tM[index] = PsycheHelper.RelativisticAddition(tM[index], kvp.Value);
        }
      }
    }
    //stopwatch.Stop();
    //PsycheHelper.CircumstanceTimings[16] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //stopwatch.Reset();

    //stopwatch.Start();
    index = PersonalityNodeMatrix.indexDict[PersonalityNodeDefOf.Cool];
    //stopwatch.Stop();
    //PsycheHelper.CircumstanceTimings[17] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //stopwatch.Reset();
    //stopwatch.Start();
    tM[index] = RelationsUtility.IsDisfigured(this.pawn) ? PsycheHelper.RelativisticAddition(tM[index], -0.1f) : tM[index];
    //stopwatch.Stop();
    //PsycheHelper.CircumstanceTimings[18] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //stopwatch.Reset();
    //stopwatch.Start();
    tM[index] = PsycheHelper.RelativisticAddition(tM[index], 0.3f * this.pawn.GetStatValue(StatDefOf.PawnBeauty));
    //stopwatch.Stop();
    //PsycheHelper.CircumstanceTimings[19] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //stopwatch.Reset();

    //stopwatch.Start();
    index = PersonalityNodeMatrix.indexDict[PersonalityNodeDefOf.LaidBack];
    foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
    {
      if (hediff.def == HediffDefOfPsychology.Anxiety)
      {
        float anxietyWeight = Mathf.Lerp(-0.5f, -0.999f, hediff.Severity);
        tM[index] = PsycheHelper.RelativisticAddition(tM[index], anxietyWeight);
        break;
      }
    }
    //stopwatch.Stop();
    //PsycheHelper.CircumstanceTimings[20] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //stopwatch.Reset();

    //stopwatch.Start();
    bool flag;
    if (applyTwice)
    {
      for (index = 0; index < PersonalityNodeMatrix.order; index++)
      {
        s = Mathf.Sign(tM[index]);
        m = Mathf.Abs(tM[index]);
        flag = m < 1f;
        tM[index] = flag ? s * (1f - Mathf.Sqrt(1f - m)) : s;
        if (flag != true)
        {
          Log.Error("Psychology: AdjustForCircumstance gave modifier with magnitude greater than or equal to 1.");
        }

      }
    }
    //stopwatch.Stop();
    //PsycheHelper.CircumstanceTimings[21] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //stopwatch.Reset();

    //stopwatch.Start();
    for (index = 0; index < PersonalityNodeMatrix.order; index++)
    {
      ratings[index] = (1f - Mathf.Abs(tM[index])) * ratings[index] + Mathf.Max(0f, tM[index]);
    }
    //stopwatch.Stop();
    //PsycheHelper.CircumstanceTimings[22] += (float)stopwatch.Elapsed.TotalMilliseconds;
    //stopwatch.Reset();
  }

  public void DeepCopyFromOtherTracker(Pawn_PsycheTracker trackerToCopy)
  {
    this.upbringing = trackerToCopy.upbringing;
    this.lastDateTick = trackerToCopy.lastDateTick;

    //this.nodes = new HashSet<PersonalityNode>();
    //foreach (PersonalityNodeDef def in PersonalityNodeMatrix.DefList)
    //{
    //  PersonalityNode n = PersonalityNodeMaker.MakeNode(def, this.pawn);
    //  this.nodes.Add(n);
    //  this.nodeDict[n.def] = n;
    //}
    //foreach (PersonalityNode n in this.nodes)
    //{
      
    //}

    foreach (PersonalityNode otherNode in trackerToCopy.nodes)
    {
      if (this.nodeDict.TryGetValue(otherNode.def, out PersonalityNode node))
      {
        node.rawRating = otherNode.rawRating;
      }
    }

    foreach (KeyValuePair<string, float> kvp in trackerToCopy.cachedOpinions)
    {
      this.cachedOpinions[kvp.Key] = kvp.Value;
    }
    foreach (KeyValuePair<string, bool> kvp in trackerToCopy.recalcCachedOpinions)
    {
      this.recalcCachedOpinions[kvp.Key] = kvp.Value;
    }
    foreach (KeyValuePair<Pair<string, string>, float> kvp in trackerToCopy.cachedDisagreementWeights)
    {
      this.cachedDisagreementWeights[new Pair<string, string>(kvp.Key.First, kvp.Key.Second)] = kvp.Value;
    }
    foreach (KeyValuePair<Pair<string, string>, bool> kvp in trackerToCopy.recalcNodeDisagreement)
    {
      this.recalcNodeDisagreement[new Pair<string, string>(kvp.Key.First, kvp.Key.Second)] = kvp.Value;
    }
    foreach (KeyValuePair<MemeDef, Dictionary<PersonalityNodeDef, float>> kvp0 in trackerToCopy.dailyCertaintyFromMemesAndNodes)
    {
      this.dailyCertaintyFromMemesAndNodes[kvp0.Key] = new Dictionary<PersonalityNodeDef, float>();
      foreach (KeyValuePair<PersonalityNodeDef, float> kvp1 in kvp0.Value)
      {
        this.dailyCertaintyFromMemesAndNodes[kvp0.Key][kvp1.Key] = kvp1.Value;
      }
    }
    foreach (KeyValuePair<PreceptDef, Dictionary<PersonalityNodeDef, float>> kvp0 in trackerToCopy.dailyCertaintyFromPerceptsAndNodes)
    {
      this.dailyCertaintyFromPerceptsAndNodes[kvp0.Key] = new Dictionary<PersonalityNodeDef, float>();
      foreach (KeyValuePair<PersonalityNodeDef, float> kvp1 in kvp0.Value)
      {
        this.dailyCertaintyFromPerceptsAndNodes[kvp0.Key][kvp1.Key] = kvp1.Value;
      }
    }
  }

  public float CalculateCertaintyChangePerDay(Ideo ideo, bool addToDicts = true)
  {
    ////Log.Message("CalculateCertaintyChangePerDay, start");
    CalculateAdjustedRatings();
    if (addToDicts)
    {
      ////Log.Message("CalculateCertaintyChangePerDay, clearing dictionaries");
      dailyCertaintyFromMemesAndNodes.Clear();
      dailyCertaintyFromPerceptsAndNodes.Clear();
    }
    if (ideo == null)
    {
      ////Log.Message("ideo is null for pawn " + pawn);
      return 0f;
    }
    //CalculateAdjustedRatings();
    float certaintyChange = 0f;
    float rating;
    float adjustment;
    MemeDef memeDef;
    PreceptDef preceptDef;
    PersonalityNodeDef nodeDef;
    foreach (KeyValuePair<MemeDef, Dictionary<PersonalityNodeDef, float>> kvp0 in PersonalityNodeIdeoUtility.memesAffectedByNodes)
    {
      memeDef = kvp0.Key;
      ////Log.Message("CalculateCertaintyChangePerDay, iterating over memesAffectedByNodes, meme def = " + memeDef.LabelCap);
      if (ideo.HasMeme(memeDef) != true)
      {
        continue;
      }
      if (addToDicts && dailyCertaintyFromMemesAndNodes.ContainsKey(memeDef) != true)
      {
        ////Log.Message("CalculateCertaintyChangePerDay, created new dictionary for meme def " + memeDef.LabelCap);
        dailyCertaintyFromMemesAndNodes[memeDef] = new Dictionary<PersonalityNodeDef, float>();
      }
      foreach (KeyValuePair<PersonalityNodeDef, float> kvp1 in kvp0.Value)
      {
        nodeDef = kvp1.Key;
        rating = GetPersonalityRating(nodeDef);
        adjustment = PsycheHelper.DailyCertaintyChangeScale * (2f * rating - 1f) * kvp1.Value;
        certaintyChange += adjustment;
        ////Log.Message("CalculateCertaintyChangePerDay, meme def " + memeDef.LabelCap + ", and node def = " + nodeDef.LabelCap);
        if (addToDicts)
        {
          ////Log.Message("CalculateCertaintyChangePerDay, added memeDef: " + memeDef.LabelCap + ", with nodeDef: " + nodeDef + ", and adjustment: " + adjustment);
          dailyCertaintyFromMemesAndNodes[memeDef][nodeDef] = adjustment;
        }
      }
    }
    foreach (KeyValuePair<PreceptDef, Dictionary<PersonalityNodeDef, float>> kvp0 in PersonalityNodeIdeoUtility.preceptsAffectedByNodes)
    {
      preceptDef = kvp0.Key;
      if (ideo.HasPrecept(preceptDef) != true)
      {
        continue;
      }
      if (addToDicts && dailyCertaintyFromPerceptsAndNodes.ContainsKey(preceptDef) != true)
      {
        dailyCertaintyFromPerceptsAndNodes[preceptDef] = new Dictionary<PersonalityNodeDef, float>();
      }
      foreach (KeyValuePair<PersonalityNodeDef, float> kvp1 in kvp0.Value)
      {
        nodeDef = kvp1.Key;
        rating = GetPersonalityRating(nodeDef);
        adjustment = PsycheHelper.DailyCertaintyChangeScale * (2f * rating - 1f) * kvp1.Value;
        certaintyChange += adjustment;
        if (addToDicts)
        {
          dailyCertaintyFromPerceptsAndNodes[preceptDef][nodeDef] = adjustment;
        }
      }
    }
    foreach (KeyValuePair<MemeDef, Dictionary<PersonalityNodeDef, float>> kvp0 in dailyCertaintyFromMemesAndNodes)
    {
      foreach (KeyValuePair<PersonalityNodeDef, float> kvp1 in kvp0.Value)
      {
        ////Log.Message("CalculateCertaintyChangePerDay, memeDef: " + kvp0.Key.LabelCap + ", with nodeDef: " + kvp1.Key + ", and adjustment: " + kvp1.Value);
      }
    }
    return certaintyChange;
  }

  public float CompatibilityWithIdeo(Ideo ideo, bool calcAdjusted = true)
  {
    if (ideo == null)
    {
      return 0f;
    }
    CalculateAdjustedRatings();
    int index;
    float[] adjDisplacementUnitVector = new float[PersonalityNodeMatrix.order];
    float norm = 0f;
    float adjDisplacement;
    foreach (PersonalityNode node in this.nodes)
    {
      index = PersonalityNodeMatrix.indexDict[node.def];
      adjDisplacement = node.AdjustedRating - 0.5f;
      adjDisplacementUnitVector[index] = adjDisplacement;
      norm += adjDisplacement * adjDisplacement;
    }
    norm = Mathf.Sqrt(norm);
    if (norm != 0f)
    {
      for (index = 0; index < adjDisplacementUnitVector.Count(); index++)
      {
        adjDisplacementUnitVector[index] /= norm;
      }
      //foreach (PersonalityNode node in this.nodes)
      //{
      //    index = PersonalityNodeMatrix.indexDict[node.def];
      //    adjDisplacementUnitVector[index] /= norm;
      //}
    }
    float[] favoredDisplacementUnitVector = PersonalityNodeIdeoUtility.FavoredDisplacmentForIdeo(ideo, randomize: false, normalize: true);
    float compat = PersonalityNodeMatrix.DotProduct(adjDisplacementUnitVector, favoredDisplacementUnitVector);
    if (Mathf.Abs(compat) > 1f)
    {
      Log.Error("Compatibility exceeded bounds of -1 to +1, compat = " + compat + ", for pawn " + this.pawn);
    }
    return compat;
  }

  public void BoostRatingsTowardsIdeo(Ideo ideo, float alpha, bool randomize)
  {
    CalculateAdjustedRatings();
    float[] newRawRatings = RatingsAfterBoostTowardsIdeo(ideo, alpha, randomize);
    foreach (PersonalityNode node in this.nodes)
    {
      node.rawRating = newRawRatings[PersonalityNodeMatrix.indexDict[node.def]];
    }
    int idnumber = pawn.thingIDNumber;
    float certaintyChange = CalculateCertaintyChangePerDay(pawn.Ideo, true);
    PsycheHelper.GameComp.CachedCertaintyChangePerDayDict[idnumber] = certaintyChange;
  }

  public float[] RatingsAfterBoostTowardsIdeo(Ideo ideo, float alpha, bool randomize = false)
  {
    float[] newRawRatings = new float[PersonalityNodeMatrix.order];
    // alpha controls the scale of the maximum increase in certainty
    float alphaTimesTwo = 2f * alpha;
    float[] mvec = PersonalityNodeIdeoUtility.FavoredDisplacmentForIdeo(ideo, randomize);
    float r;
    int index;
    float m;
    float u;
    float x;
    foreach (PersonalityNode node in this.nodes)
    {
      r = node.rawRating;
      index = PersonalityNodeMatrix.indexDict[node.def];
      m = mvec[index];
      if (m == 0f)
      {
        newRawRatings[index] = r;
        continue;
      }
      u = m > 0f ? 1f : 0f;
      x = alphaTimesTwo * Mathf.Abs(m) * PsycheHelper.NormalCDFInv(0.5f + 0.5f * (u - r));
      newRawRatings[index] = PsycheHelper.NormalCDF(PsycheHelper.NormalCDFInv(r) + x);
    }
    return newRawRatings;
  }

  //public string IdeoAbilityEffectOnPsycheTooltip(float multiplier)
  //{
  //    Ideo ideo = this.pawn.Ideo;
  //    int index;
  //    // Calculate current compatibility 
  //    CalculateAdjustedRatings();
  //    float oldDailyChange = CalculateCertaintyChangePerDay(ideo, true);
  //    float[] oldRawRatings = new float[PersonalityNodeMatrix.order];
  //    //float[] oldAdjRatings = new float[PersonalityNodeMatrix.order];

  //    float[] newRawRatings = RatingsAfterBoostTowardsIdeo(ideo, multiplier, true);
  //    //float[] newAdjRatings = new float[PersonalityNodeMatrix.order];
  //    foreach (PersonalityNode node in this.nodes)
  //    {
  //        index = PersonalityNodeMatrix.indexDict[node.def];
  //        // Store old raw ratings
  //        oldRawRatings[index] = node.rawRating;
  //        // Store old adjusted ratings
  //        //oldAdjRatings[index] = node.AdjustedRating;
  //        // Set raw to new ratings
  //        node.rawRating = newRawRatings[index];
  //    }
  //    // Calculate new adjusted ratings and compatibility
  //    CalculateAdjustedRatings();
  //    float newDailyChange = CalculateCertaintyChangePerDay(ideo, false);
  //    foreach (PersonalityNode node in this.nodes)
  //    {
  //        //index = PersonalityNodeMatrix.indexDict[node.def];
  //        // Store new adjusted ratings
  //        //newAdjRatings[index] = node.AdjustedRating;
  //        // Restore old raw ratings
  //        //node.rawRating = oldRawRatings[index];
  //    }
  //    // Restore old adjusted ratings
  //    CalculateAdjustedRatings();

  //    //List<Pair<PersonalityNodeDef, float>> list = new List<Pair<PersonalityNodeDef, float>>();
  //    //foreach (PersonalityNodeDef nodeDef in PersonalityNodeMatrix.defList)
  //    //{
  //    //    //index = PersonalityNodeMatrix.indexDict[nodeDef];
  //    //    //list.Add(new Pair<PersonalityNodeDef, float>(nodeDef, newAdjRatings[index] - oldAdjRatings[index]));
  //    //}
  //    //list.OrderBy(x => -Mathf.Abs(x.Second));

  //    //string text = "";
  //    //text += "Counseling will alter {PAWN_possessive} personality to be more compatible with {IDEO}.";
  //    //text += "\n\nEffects on psyche";
  //    //text += "\n -  Certainty change per day increases on average by " + (newCompat - oldCompat).ToStringPercent();

  //    string oldPercent = (oldDailyChange > 0f ? "+" : "") + oldDailyChange.ToStringPercent();
  //    string newPercent = (newDailyChange > 0f ? "+" : "") + newDailyChange.ToStringPercent();

  //    // ToDo: turn this into translation
  //    string text = "Effect on daily certainty change due to personality:\nShould increase from " + oldPercent + " to " + newPercent + ", up to some randomness.";
  //    if (newDailyChange < oldDailyChange)
  //    {
  //        Log.Error("New daily change is lower than old daily change");
  //    }
  //    return text;
  //}

}


