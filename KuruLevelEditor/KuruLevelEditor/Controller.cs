using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace KuruLevelEditor
{
    class Controller
    {
		public enum Action
		{
			LEFT = 0,
			RIGHT,
			TOP,
			BOTTOM,
			ZOOM_IN,
			ZOOM_OUT
		}

		static readonly TimeSpan MOVE_DELAY = new TimeSpan(0, 0, 0, 0, 50);
		static readonly TimeSpan ZOOM_DELAY = new TimeSpan(0, 0, 0, 0, 50);

		static TimeSpan last_direction_time = TimeSpan.Zero;
		static TimeSpan last_zoom_time = TimeSpan.Zero;
		static int last_scroll_wheel_value = 0;
		public static List<Action> GetActionsGrid(KeyboardState state, MouseState mouse, GameTime gt)
		{
			List<Action> actions = new List<Action>();
			TimeSpan total_time = gt.TotalGameTime;

			if (last_direction_time.Add(MOVE_DELAY).CompareTo(total_time) <= 0)
            {
				if (state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.Right) || state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.Down))
                {
					last_direction_time = total_time;
					if (state.IsKeyDown(Keys.Left))
						actions.Add(Action.LEFT);
					if (state.IsKeyDown(Keys.Down))
						actions.Add(Action.BOTTOM);
					if (state.IsKeyDown(Keys.Right))
						actions.Add(Action.RIGHT);
					if (state.IsKeyDown(Keys.Up))
						actions.Add(Action.TOP);
				}
            }

			if (state.IsKeyDown(Keys.LeftControl))
            {
				if (mouse.ScrollWheelValue > last_scroll_wheel_value)
					actions.Add(Action.ZOOM_IN);
				if (mouse.ScrollWheelValue < last_scroll_wheel_value)
					actions.Add(Action.ZOOM_OUT);

				if (last_zoom_time.Add(ZOOM_DELAY).CompareTo(total_time) <= 0)
				{
					if (state.IsKeyDown(Keys.OemPlus) || state.IsKeyDown(Keys.OemMinus))
					{
						last_zoom_time = total_time;
						if (state.IsKeyDown(Keys.OemPlus))
							actions.Add(Action.ZOOM_IN);
						if (state.IsKeyDown(Keys.OemMinus))
							actions.Add(Action.ZOOM_OUT);
					}
				}
			}
			last_scroll_wheel_value = mouse.ScrollWheelValue;

			return actions;
		}

	}
}
