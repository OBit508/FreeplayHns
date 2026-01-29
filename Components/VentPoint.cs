using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FreeplayHns.Components
{
    internal class VentPoint : Point
    {
        public Vent vent;
        public VentPoint(Vent vent, bool Connect = true, bool IgnoreColliders = false, float ConnectDistance = 0.5f) : base(vent.transform.position, Connect, IgnoreColliders, ConnectDistance)
        {
            this.vent = vent;
        }
    }
}
