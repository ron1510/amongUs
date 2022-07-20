using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Vent
    {
        static int ventCounter = 0;
        static Vent[] vents = null;
        public static List<Vent>[] travelToVents;

        public Vector2 ventPos;
        public List<Vent> possibleVents;
        public int ventID;

        public Vent(Vector2 pos) 
        {
            ventID = ventCounter;
            ventCounter++;
            possibleVents = new List<Vent>();
            ventPos = pos;
        }

        public Vent(Vector2 pos, int id):this(pos)
        {
            this.ventID = id;
        }

        public static Vent [] GetVents() 
        {
            if (Vent.vents == null) 
            {
                GameObject[] ventsObject = GameObject.FindGameObjectsWithTag("Vent");
                Vent[] vents = new Vent[ventsObject.Length];
                for (int i = 0; i < vents.Length; i++)
                {
                    vents[i] = new Vent(ventsObject[i].transform.position,int.Parse(ventsObject[i].name.Substring(10, ventsObject[i].name.Length-10)));
                }
                for (int i = 0; i < vents.Length; i++)
                {
                    for (int j = 0; j < vents.Length; j++)
                    {
                        if (Vector2.Distance(vents[i].ventPos, vents[j].ventPos) <= 0.5 && vents[i] != vents[j])
                        {
                            vents[i].possibleVents.Add(vents[j]);
                        }
                    }
                }
                Vent.vents = vents;
                return vents;
            }
            return vents;
        }
        public static void SetTravelVents() 
        {
            Vent[] vetns = GetVents();
            travelToVents = new List<Vent>[14];
            for (int i = 0; i < 14; i++)
            {
                travelToVents[i] = new List<Vent>();
            }
            travelToVents[0].Add(GetVentByID(6));

            travelToVents[1].Add(GetVentByID(9));
            travelToVents[1].Add(GetVentByID(11));

            travelToVents[2].Add(GetVentByID(7));

            travelToVents[3].Add(GetVentByID(13));

            travelToVents[4].Add(GetVentByID(14));
            travelToVents[4].Add(GetVentByID(10));

            travelToVents[5].Add(GetVentByID(1));

            travelToVents[6].Add(GetVentByID(3));

            travelToVents[7].Add(GetVentByID(12));

            travelToVents[8].Add(GetVentByID(2));
            travelToVents[8].Add(GetVentByID(11));

            travelToVents[9].Add(GetVentByID(5));
            travelToVents[9].Add(GetVentByID(14));

            travelToVents[10].Add(GetVentByID(9));
            travelToVents[10].Add(GetVentByID(2));

            travelToVents[11].Add(GetVentByID(8));

            travelToVents[12].Add(GetVentByID(4));

            travelToVents[13].Add(GetVentByID(10));
            travelToVents[13].Add(GetVentByID(5));
        }

        public static Vent GetVentByID(int id) 
        {
            Vent[] vetns = GetVents();
            for (int i = 0; i < vents.Length; i++)
            {
                if (vents[i].ventID == id)
                {
                    return vents[i];
                }
            }
            return null;
        }
    }
}
