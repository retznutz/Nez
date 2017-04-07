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
        float _minBoundary = 0;
        float _maxBoundary = 0;


        float zoomStep = .1f;
        float zoomLerp = .2f;
        float percentBoundary = .4f;

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
                camera.minimumZoom = 0.1f;
                camera.maximumZoom = 100f;

                _minBoundary = 1.0f - percentBoundary;
                _maxBoundary = 1.0f + percentBoundary;
            }

            follow(_targetEntities);

            // listen for changes in screen size
            Core.emitter.addObserver(CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset);
        }


        public override void onRemovedFromEntity()
        {
            Core.emitter.removeObserver(CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset);
        }


        float _currentZoomTarget;

        void IUpdatable.update()
        {

            _worldSpaceDeadzone.x = camera.position.X + focusOffset.X;
            _worldSpaceDeadzone.y = camera.position.Y + focusOffset.Y;

            if (_targetEntities != null)
                updateFollow();
                
            camera.position = Vector2.Lerp(camera.position, camera.position + _desiredPositionDelta, followLerp);
            camera.entity.transform.roundPosition();


            updateZoom(); //1-f^dt



            System.Diagnostics.Debug.WriteLine(camera.zoom.ToString() + " : " + _currentZoomTarget.ToString() + " : " + MathHelper.Lerp(camera.zoom, _currentZoomTarget, Time.deltaTime * 5.5f).ToString());
            camera.zoom = MathHelper.Lerp(camera.zoom, _currentZoomTarget, Time.deltaTime * 5.5f);

            
        }


        void updateZoom()
        {
            //float zoomLevel = camera.rawZoom;

            //0 = no zoom
            //-1 = zoom out
            //1 = zoom in

           // _currentZoomTarget = 0;

            //this will be between 0% and ???%
            float widthDifferencePercent = _entityRegion.width / camera.bounds.width;
            
            if (widthDifferencePercent > 0.8f)
            {
                //zoom out
                _currentZoomTarget = camera.zoom - 0.5f;
             //   System.Diagnostics.Debug.WriteLine(_currentZoomTarget.ToString() + "out");
            }
            else if (widthDifferencePercent < 0.6f)
            {
                _currentZoomTarget = camera.zoom + 0.5f;
                //System.Diagnostics.Debug.WriteLine(_currentZoomTarget.ToString() + "in");
            }
            else if (widthDifferencePercent >= 0.6f && _currentZoomTarget <= 0.8f)
            {
                _currentZoomTarget = camera.zoom;
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
