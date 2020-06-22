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
			FLIP_VERTICAL,
			TOGGLE_INVENTORY
		}

		static readonly TimeSpan MOVE_DELAY = new TimeSpan(0, 0, 0, 0, 50);
		static readonly TimeSpan ZOOM_DELAY = new TimeSpan(0, 0, 0, 0, 50);
		static readonly TimeSpan BRUSH_DELAY = new TimeSpan(0, 0, 0, 0, 100);
		static readonly TimeSpan FLIP_DELAY = new TimeSpan(0, 0, 0, 0, 500);
		static readonly TimeSpan INVENTORY_DELAY = new TimeSpan(0, 0, 0, 0, 500);

		static TimeSpan last_direction_time = TimeSpan.Zero;
		static TimeSpan last_zoom_time = TimeSpan.Zero;
		static TimeSpan last_brush_time = TimeSpan.Zero;
		static TimeSpan last_flip_time = TimeSpan.Zero;
		static TimeSpan last_inventory_time = TimeSpan.Zero;
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
				
				if (state.IsKeyDown(Keys.OemPlus) || state.IsKeyDown(Keys.OemMinus))
				{
					if (last_zoom_time.Add(ZOOM_DELAY) <= total_time)
					{
						last_zoom_time = total_time;
						if (state.IsKeyDown(Keys.OemPlus))
							actions.Add(Action.ZOOM_IN);
						if (state.IsKeyDown(Keys.OemMinus))
							actions.Add(Action.ZOOM_OUT);
					}
				}
				else
					last_zoom_time = TimeSpan.Zero;
			}
			else if (state.IsKeyDown(Keys.LeftAlt))
			{
				if (mouse.ScrollWheelValue > last_scroll_wheel_value)
					actions.Add(Action.BRUSH_PLUS);
				if (mouse.ScrollWheelValue < last_scroll_wheel_value)
					actions.Add(Action.BRUSH_MINUS);

				if (state.IsKeyDown(Keys.OemPlus) || state.IsKeyDown(Keys.OemMinus))
				{
					if (last_brush_time.Add(BRUSH_DELAY) <= total_time)
					{
						last_brush_time = total_time;
						if (state.IsKeyDown(Keys.OemPlus))
							actions.Add(Action.BRUSH_PLUS);
						if (state.IsKeyDown(Keys.OemMinus))
							actions.Add(Action.BRUSH_MINUS);
					}
				}
				else
					last_brush_time = TimeSpan.Zero;
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

				if (state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.Right) || state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.Down))
				{
					if (last_flip_time.Add(FLIP_DELAY) <= total_time)
					{
						last_flip_time = total_time;
						if (state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.Right))
							actions.Add(Action.FLIP_HORIZONTAL);
						if (state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.Down))
							actions.Add(Action.FLIP_VERTICAL);
					}
				}
				else
					last_flip_time = TimeSpan.Zero;
			}
			else
            {
				if (mouse.ScrollWheelValue > last_scroll_wheel_value)
					actions.Add(Action.SELECT_PREVIOUS);
				if (mouse.ScrollWheelValue < last_scroll_wheel_value)
					actions.Add(Action.SELECT_NEXT);

				if (state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.Right) || state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.Down))
				{
					if (last_direction_time.Add(MOVE_DELAY) <= total_time)
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
				else
					last_direction_time = TimeSpan.Zero;
				
				if (state.IsKeyDown(Keys.Space))
				{
					if (last_inventory_time.Add(INVENTORY_DELAY) <= total_time)
					{
						last_inventory_time = total_time;
						actions.Add(Action.TOGGLE_INVENTORY);
					}
				}
				else
					last_inventory_time = TimeSpan.Zero;
			}

			last_scroll_wheel_value = mouse.ScrollWheelValue;

			return actions;
		}

	}
}
