using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LnzDecoder {
    public class Addball
    {
        public int x, y, z, parent, size;
    }

    public class Line
    {
        public int parent1, parent2;
    }

    public List<Addball> addballs = new List<Addball>();
    public List<Line> lines = new List<Line>();
    
    public LnzDecoder(string [] data, PetzAnimationDecoder decoder)
    {
        char[] delims = new char[] { ' ', '\t',',' };
        //do addballs
        int index = -1;
        for(int i = 0; i < data.Length; i++) if(data[i]=="[Add Ball]")
            {
                index = i+1;
                break;
            }
        for(int i = index; i < data.Length; i++)
        {
            string[] s = data[i].Split(delims, StringSplitOptions.RemoveEmptyEntries);
            if (data[i].Length == 0) continue;
            if (s[0][0] == '[')
            {
                break;
            }
            if (s[0][0]!=';')
            {
                Addball a = new Addball();
                a.parent = int.Parse(s[0]);
                a.x = int.Parse(s[1]);
                a.y = int.Parse(s[2]);
                a.z = int.Parse(s[3]);
                a.size = int.Parse(s[10]);
                addballs.Add(a);
            }
        }
        //do omissions
        for (int i = 0; i < data.Length; i++)
        {
            string[] s = data[i].Split(delims, StringSplitOptions.RemoveEmptyEntries);
            if (s.Length == 0) continue;
            if (s[0] == "[Omissions]")
            {
                index = i+1;
                break;
            }
        }
        for (int i = index; i < data.Length; i++)
        {
            string[] s = data[i].Split(delims, StringSplitOptions.RemoveEmptyEntries);
            if (data[i].Length == 0) continue;
            if (s[0][0] == '[')
            {
                break;
            }
            if (s[0][0] != ';')
            {
                int j = int.Parse(s[0]);
                if (j < decoder.numBalls)
                {
                    decoder.ballSizes[j] = 0;
                }
                else
                {
                    addballs[j - decoder.numBalls].size = 0;
                }
            }
        }
        //do lines
        for (int i = 0; i < data.Length; i++)
        {
            string[] s = data[i].Split(delims, StringSplitOptions.RemoveEmptyEntries);
            if (s.Length == 0) continue;
            if (s[0] == "[Linez]")
            {
                index = i+1;
                break;
            }
        }
        for (int i = index; i < data.Length; i++)
        {
            string[] s = data[i].Split(delims, StringSplitOptions.RemoveEmptyEntries);
            if (data[i].Length == 0) continue;
            if (s[0][0] == '[')
            {
                break;
            }
            if (s[0][0] != ';')
            {
                Line line = new Line();
                line.parent1 = int.Parse(s[0]);
                line.parent2 = int.Parse(s[1]);
                lines.Add(line);
            }
        }
        /*for (int i = 0; i < data.Length; i++)
        {
            string[] s = data[i].Split(delims,StringSplitOptions.RemoveEmptyEntries);
            if (data[i].Length == 0) continue;
            if(data[i][0]=='[')
            {
                if (data[i]== "[Add Ball]")
                {
                    state = LnzState.Addball;
                } else if(s[0]=="[Omissions]")
                {
                    state = LnzState.Omissions;
                } else if(s[0]=="[Linez]")
                {
                    state = LnzState.Line;
                }
                else state = LnzState.None;
            } else if(data[i][0]!=';')
            {
                switch (state)
                {
                    case LnzState.Addball:
                        {
                            Addball a = new Addball();
                            a.parent = int.Parse(s[0]);
                            a.x = int.Parse(s[1]);
                            a.y = int.Parse(s[2]);
                            a.z = int.Parse(s[3]);
                            a.size = int.Parse(s[10]);
                            addballs.Add(a);
                        }
                        break;
                    case LnzState.Omissions:
                        {
                            int index = int.Parse(s[0]);
                            if(index<decoder.numBalls)
                            {
                                decoder.ballSizes[index] = 0;
                            } else
                            {
                                addballs[index - decoder.numBalls].size = 0;
                            }
                        }
                        break;
                    case LnzState.Line:
                        {
                            Line line = new Line();
                            line.parent1 = int.Parse(s[0]);
                            line.parent2 = int.Parse(s[1]);
                            lines.Add(line);
                        }
                        break;
                    case LnzState.None:
                    default:
                        break;
                }

            }
        }*/
    }
}
