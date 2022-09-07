using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Psychology
{
    public class PersonalityNodeParent
    {
        //The parent node this node descends from.
        public PersonalityNodeDef node;
        // How the parent node modifies this node.
        public float modifier;
    }
}
