/*
    Copyright (C) 2021 CodeStrikers.org
    This file is part of NETReactorSlayer.
    NETReactorSlayer is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    NETReactorSlayer is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with NETReactorSlayer.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Drawing;
using System.Windows.Forms;

namespace NETReactorSlayer.GUI;

public class MenuColorTable : ProfessionalColorTable
{
    public MenuColorTable() => UseSystemColors = false;

    public override Color MenuBorder => Color.Black;

    public override Color MenuItemBorder => Color.Black;

    public override Color MenuItemSelected => Color.FromArgb(18, 18, 18);
}