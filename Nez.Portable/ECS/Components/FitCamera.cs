using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Nez.ECS.Components
{
    public class FitCamera : Component, IUpdatable
    {

        public Camera camera;
        public float followLerp = 0.2f;

        List<Entity> _targetEntities;
        Vector2 _desiredPositionDelta;
        RectangleF _worldSpaceDeadzone;
        public Vector2 focusOffset;
        RectangleF _entityRegion;
        float _currentZoom = .1f;

        public FitCamera(List<Entity> targetEntities, Camera camera)
        {
            _targetEntities = targetEntities;
            this.camera = camera;

          
        }


        public FitCamera(List<Entity> targetEntities) : this(targetEntities, null )
		{ }



        public override void onAddedToEntity()
        {
            if (camera == null)
            {
                camera = entity.scene.camera;
                //camera.minimumZoom = 3;
                //camera.maximumZoom = 10;
            }


            follow(_targetEntities);

            // listen for changes in screen size so we can keep our deadzone properly positioned
            Core.emitter.addObserver(CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset);
        }


        public override void onRemovedFromEntity()
        {
            Core.emitter.removeObserver(CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset);
        }



        void IUpdatable.update()
        {
            // translate the deadzone to be in world space

            _worldSpaceDeadzone.x = camera.position.X + focusOffset.X;
            _worldSpaceDeadzone.y = camera.position.Y + focusOffset.Y;

            if (_targetEntities != null)
                updateFollow();

            camera.position = Vector2.Lerp(camera.position, camera.position + _desiredPositionDelta, followLerp);
            camera.entity.transform.roundPosition();


            if (_entityRegion.top < camera.bounds.top || _entityRegion.bottom  > camera.bounds.bottom || _entityRegion.left  < camera.bounds.left || _entityRegion.right  > camera.bounds.right)
            {
                camera.zoomOut(0.01f);
            }
            else if (_entityRegion.top > camera.bounds.top || _entityRegion.bottom  < camera.bounds.bottom || _entityRegion.left  > camera.bounds.left || _entityRegion.right  < camera.bounds.right)
            {
                camera.zoomIn(0.01f);
            }




            //    if (_entityRegion.left < camera.bounds.left || _entityRegion.right > camera.bounds.right)
            //{
            //    camera.zoomOut(0.005f);
            //}
            //else if (_entityRegion.left > camera.bounds.left || _entityRegion.right  < camera.bounds.right)
            //{
            //    camera.zoomIn(0.005f);
            //}

            //if (_entityRegion.width > _entityRegion.height)
            //{
            //    camera.zoom = -(_entityRegion.width / camera.bounds.width);
            //}
            //else
            //{
            //    camera.zoom = -(_entityRegion.height / camera.bounds.height);
            //}

            //System.Diagnostics.Debug.WriteLine(camera.rawZoom);
          
            // camera.zoomOut(_currentZoom);
            //camera.
        }



        void updateFollow()
        {
            _desiredPositionDelta.X = _desiredPositionDelta.Y = 0;

            _entityRegion = findMinMaxRect(_targetEntities.Select(d => d.transform).ToList());
            Vector2 target = _entityRegion.center;


            var targetX = target.X;
            var targetY = target.Y;

            // x-axis
            if (_worldSpaceDeadzone.x > targetX)
                _desiredPositionDelta.X = targetX - _worldSpaceDeadzone.x;
            else if (_worldSpaceDeadzone.x < targetX)
                _desiredPositionDelta.X = targetX - _worldSpaceDeadzone.x;

            // y-axis
            if (_worldSpaceDeadzone.y < targetY)
                _desiredPositionDelta.Y = targetY - _worldSpaceDeadzone.y;
            else if (_worldSpaceDeadzone.y > targetY)
                _desiredPositionDelta.Y = targetY - _worldSpaceDeadzone.y;

        }


        public void follow(List<Entity> targetEntities)
        {
            _targetEntities = targetEntities;
           // var cameraBounds = camera.bounds;
        }



        private RectangleF findMinMaxRect(IList<Transform> targets)
        {

            
            Vector2 minPoint = targets[0].position;
            Vector2 maxPoint = targets[0].position;

            for (int i = 1; i < targets.Count; i++)
            {
                Vector2 pos = targets[i].position;
                if (pos.X < minPoint.X)
                    minPoint.X = pos.X;
                if (pos.X > maxPoint.X)
                    maxPoint.X = pos.X;
                if (pos.Y < minPoint.Y)
                    minPoint.Y = pos.Y;
                if (pos.Y > maxPoint.Y)
                    maxPoint.Y = pos.Y;
              
            }

       
            return new RectangleF(minPoint, maxPoint - minPoint);

            

        }

        //private Vector2 findCentroid(RectangleF rect)
        //{
        //    Vector2 centroid;

        //    centroid = minPoint + 0.5f * (maxPoint - minPoint);

        //    return centroid;
        //}



        void onGraphicsDeviceReset()
        {
            // we need this to occur on the next frame so the camera bounds are updated
            Core.schedule(0f, this, t =>
            {
                var self = t.context as FitCamera;
                self.follow(self._targetEntities);
            });
        }




    }
}
