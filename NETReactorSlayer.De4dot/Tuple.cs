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

namespace NETReactorSlayer.De4dot {
    public class Tuple<T1, T2> {
        public Tuple(T1 item1, T2 item2) {
            Item1 = item1;
            Item2 = item2;
        }

        public override bool Equals(object obj) {
            if (obj is not Tuple<T1, T2> other)
                return false;
            return Item1.Equals(other.Item1) && Item2.Equals(other.Item2);
        }

        public override int GetHashCode() => Item1.GetHashCode() + Item2.GetHashCode();

        public override string ToString() => "<" + Item1 + "," + Item2 + ">";

        public T1 Item1 { get; }
        public T2 Item2 { get; }
    }
}