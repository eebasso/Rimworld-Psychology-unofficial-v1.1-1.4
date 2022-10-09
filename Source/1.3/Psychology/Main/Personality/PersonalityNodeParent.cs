using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Psychology;

public class PersonalityNodeParent
{
    //The parent node that correlates with this node.
    public PersonalityNodeDef node;
    // How the parent node correlates with this node.
    public float modifier;
}
