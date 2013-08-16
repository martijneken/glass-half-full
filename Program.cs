/*
 * Copyright (C) 2013 Martijn Stevenson
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlassHalfFull
{
    class Program
    {
        static int TARGET = 6; // 7
        static int[] GLASSES = new int[] { 4, 9 }; // { 3, 8, 17 }

        static void Main(string[] args)
        {
            // setup
            Queue<State> states = new Queue<State>();
            State start = new State() { fills = new int[GLASSES.Length], steps = new string[0] };
            states.Enqueue(start);

            // BFS (breadth first search) ensures shortest path
            int tries = 0;
            int solutions = 0;
            var pastStates = new HashSet<string>();
            while (states.Count > 0)
            {
                tries++;
                State current = states.Dequeue();
                
                // check for answer
                if (current.fills.Any(f => f == TARGET))
                {
                    solutions++;
                    Console.WriteLine("Found solution! {0} tries", tries);
                    int i = 0;
                    foreach (string step in current.steps)
                    {
                        Console.WriteLine(string.Format("step {0}: {1}", ++i, step));
                    }
                    continue; // found solution, stop exploring
                }

                // ensure that there are no loops, and ensure
                //  that we don't do repeat work (fill + dump loops, etc)
                // this is an important optimization even if you stop at the first
                //  solution you find (from 4454 to 59 tries)
                string stateHash = current.StateString();
                if (pastStates.Contains(stateHash)) continue; // seen this before, stop exploring
                pastStates.Add(stateHash);

                // queue up actions
                // - fill any glass
                // - empty any glass
                // - pour any glass into any other glass
                for (int g = 0; g < GLASSES.Length; g++)
                {
                    int fillg = current.fills[g];
                    int sizeg = GLASSES[g];

                    // fill
                    if (fillg < sizeg) // not already full
                    {
                        states.Enqueue(current.Step(
                            string.Format("Fill glass {0} (+ {1})", g + 1, sizeg - fillg),
                            g, sizeg, null, null));
                    }

                    // empty
                    if (fillg > 0) // not already empty
                    {
                        states.Enqueue(current.Step(
                            string.Format("Dump glass {0} (- {1})", g + 1, fillg),
                            g, 0, null, null));
                    }

                    // transfer
                    for (int o = 0; o < GLASSES.Length; o++)
                    {
                        if (o == g) continue; // same glass

                        int fillo = current.fills[o];
                        int sizeo = GLASSES[o];
                        int pour = Math.Min(fillg, sizeo - fillo);
                        if (pour == 0) continue; // current glass empty, or other glass full

                        states.Enqueue(current.Step(
                            string.Format("Pour glass {0} into glass {1} (+/- {2})", g + 1, o + 1, pour),
                            g, fillg - pour, o, fillo + pour));
                    }
                }
            }
            Console.WriteLine("{0} solutions, {1} tries", solutions, tries);
        }

        class State
        {
            public int[] fills;
            public string[] steps;

            /// <summary>
            /// Display string / unique hash for the fills state
            /// </summary>
            public string StateString()
            {
                var sb = new StringBuilder();
                sb.Append("[");
                for (int i = 0; i < fills.Length; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append(fills[i]);
                }
                sb.Append("]");
                return sb.ToString();
            }

            /// <summary>
            /// From this state, return a new state with 1 or 2 different fills
            /// </summary>
            public State Step(string desc, int glass1, int fill1, int? glass2, int? fill2)
            {
                // copy
                var s = new State()
                {
                    fills = new int[this.fills.Length],
                    steps = new string[this.steps.Length + 1],
                };
                this.fills.CopyTo(s.fills, 0);
                this.steps.CopyTo(s.steps, 0);

                // mutate fills
                s.fills[glass1] = fill1;
                if (glass2.HasValue)
                    s.fills[glass2.Value] = fill2.Value;

                // add descriptive step summary
                s.steps[s.steps.Length - 1] = string.Format("{0} - {1}", s.StateString(), desc);

                return s;
            }
        }
    }
}
