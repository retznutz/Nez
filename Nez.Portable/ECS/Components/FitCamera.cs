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
        public float zoomBoundaryWidth = 600f;
        public float zoomBoundaryHeight = 600f;
        public float zoomSpeed = 5.5f;

        List<Entity> _targetEntities;
        Vector2 _desiredPositionDelta;
        RectangleF _worldSpaceDeadzone;
        public Vector2 focusOffset;
        RectangleF _entityRegion;
        float _currentZoomTarget;

       

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
            }

            follow(_targetEntities);

            // listen for changes in screen size
            Core.emitter.addObserver(CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset);
        }


        public override void onRemovedFromEntity()
        {
            Core.emitter.removeObserver(CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset);
        }


        

        void IUpdatable.update()
        {

            _worldSpaceDeadzone.x = camera.position.X + focusOffset.X;
            _worldSpaceDeadzone.y = camera.position.Y + focusOffset.Y;

            if (_targetEntities != null)
            {
                updateFollow();
                updateZoom();
            }

            camera.position = Vector2.Lerp(camera.position, camera.position + _desiredPositionDelta, followLerp);
            camera.zoom = MathHelper.Lerp(camera.zoom, camera.zoom + _currentZoomTarget, Time.deltaTime * zoomSpeed);  //1-f^dt

            camera.entity.transform.roundPosition();
        }



        void updateZoom()
        {

            float widthDifference = camera.bounds.width - _entityRegion.width - zoomBoundaryWidth;
            float heightDifference = camera.bounds.height - _entityRegion.height - zoomBoundaryHeight;

            if (widthDifference < heightDifference)
            {
                _currentZoomTarget = widthDifference * 0.001f; //scale factor
            }
            else
            {
                _currentZoomTarget = heightDifference * 0.001f;
            }

        }




        void updateFollow()
        {
            _desiredPositionDelta.X = _desiredPositionDelta.Y = 0;

            _entityRegion = findMinMaxRect(_targetEntities.Select(d => d.transform).ToList());

            var targetX = _entityRegion.center.X;
            var targetY = _entityRegion.center.Y;

            if (_worldSpaceDeadzone.x > targetX)
                _desiredPositionDelta.X = targetX - _worldSpaceDeadzone.x;
            else if (_worldSpaceDeadzone.x < targetX)
                _desiredPositionDelta.X = targetX - _worldSpaceDeadzone.x;

            if (_worldSpaceDeadzone.y < targetY)
                _desiredPositionDelta.Y = targetY - _worldSpaceDeadzone.y;
            else if (_worldSpaceDeadzone.y > targetY)
                _desiredPositionDelta.Y = targetY - _worldSpaceDeadzone.y;

        }


        public void follow(List<Entity> targetEntities)
        {
            _targetEntities = targetEntities;
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
