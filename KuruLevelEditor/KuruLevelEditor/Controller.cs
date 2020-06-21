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
			ZOOM_OUT,
			SELECT_NEXT,
			SELECT_PREVIOUS,
			BRUSH_PLUS,
			BRUSH_MINUS,
			FLIP_HORIZONTAL,
			FLIP_VERTICAL
		}

		static readonly TimeSpan MOVE_DELAY = new TimeSpan(0, 0, 0, 0, 50);
		static readonly TimeSpan ZOOM_DELAY = new TimeSpan(0, 0, 0, 0, 50);
		static readonly TimeSpan BRUSH_DELAY = new TimeSpan(0, 0, 0, 0, 100);
		static readonly TimeSpan FLIP_DELAY = new TimeSpan(0, 0, 0, 0, 500);

		static TimeSpan last_direction_time = TimeSpan.Zero;
		static TimeSpan last_zoom_time = TimeSpan.Zero;
		static TimeSpan last_brush_time = TimeSpan.Zero;
		static TimeSpan last_flip_time = TimeSpan.Zero;
		static int last_scroll_wheel_value = 0;
		static bool last_flip_direction = false;
		static bool last_flip_wheel_direction = false;
		public static List<Action> GetActionsGrid(KeyboardState state, MouseState mouse, GameTime gt)
		{
			List<Action> actions = new List<Action>();
			TimeSpan total_time = gt.TotalGameTime;

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
			else if (state.IsKeyDown(Keys.LeftAlt))
			{
				if (mouse.ScrollWheelValue > last_scroll_wheel_value)
					actions.Add(Action.BRUSH_PLUS);
				if (mouse.ScrollWheelValue < last_scroll_wheel_value)
					actions.Add(Action.BRUSH_MINUS);

				if (last_brush_time.Add(BRUSH_DELAY).CompareTo(total_time) <= 0)
				{
					if (state.IsKeyDown(Keys.OemPlus) || state.IsKeyDown(Keys.OemMinus))
					{
						last_brush_time = total_time;
						if (state.IsKeyDown(Keys.OemPlus))
							actions.Add(Action.BRUSH_PLUS);
						if (state.IsKeyDown(Keys.OemMinus))
							actions.Add(Action.BRUSH_MINUS);
					}
				}
			}
			else if (state.IsKeyDown(Keys.LeftShift))
            {
				if (mouse.ScrollWheelValue != last_scroll_wheel_value)
				{
					bool wheel_direction = mouse.ScrollWheelValue > last_scroll_wheel_value;
					if (wheel_direction == last_flip_wheel_direction)
						last_flip_direction = !last_flip_direction;
					else
						last_flip_wheel_direction = wheel_direction;

					if (last_flip_direction)
						actions.Add(Action.FLIP_HORIZONTAL);
					else
						actions.Add(Action.FLIP_VERTICAL);
				}

				if (last_flip_time.Add(FLIP_DELAY).CompareTo(total_time) <= 0)
				{
					if (state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.Right) || state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.Down))
					{
						last_flip_time = total_time;
						if (state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.Right))
							actions.Add(Action.FLIP_HORIZONTAL);
						if (state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.Down))
							actions.Add(Action.FLIP_VERTICAL);
					}
				}
			}
			else
            {
				if (mouse.ScrollWheelValue > last_scroll_wheel_value)
					actions.Add(Action.SELECT_PREVIOUS);
				if (mouse.ScrollWheelValue < last_scroll_wheel_value)
					actions.Add(Action.SELECT_NEXT);

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
			}

			last_scroll_wheel_value = mouse.ScrollWheelValue;

			return actions;
		}

	}
}
