using UnityEngine;

namespace BubbleField
{
    public class BubbleFieldScrollService  : IBubbleFieldScrollService
    {
        private readonly BubbleFieldGrid _grid;
        private bool _initialized;
        private int _prevGround;

        public BubbleFieldScrollService (BubbleFieldGrid grid)
        {
            _grid = grid;
        }

        public void Init()
        {
            if (_grid == null) return;
            _prevGround = _grid.GetGroundRow();
            _initialized = true;
        }

        public void OnShotResolved()
        {
            if (_grid== null) return;
            if(!_initialized) Init();
            
            int newGround = _grid.GetGroundRow();
            int diff = newGround - _prevGround;
            if(diff != 0)
                _grid.ShiftOriginRows(diff);
            
            _prevGround = newGround;
        }
    }
}