using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez
{
    public class CameraZoom : Component, IUpdatable
    {


        public void zoomTo(RectangleF rect)
        {


        }




        void IUpdatable.update()
        {


            var camera = entity.scene.camera;
        }


    }




}
