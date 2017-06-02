﻿using System;
namespace ArmyAnt.ArmyAnt.Utility.Json
{
	public interface IJsonCollection : IUnit, IEnumerable<IUnit>
	{
		bool AddChild(IUnit child, string tag);
		bool RemoveChild(string tag);
		IUnit GetChild(string tag);

		int Length { get; }
	}

}
