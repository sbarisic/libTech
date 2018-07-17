using System;
using System.Collections.Generic;

// https://github.com/ChevyRay/RectanglePacker

namespace libTech {
	public class RectanglePacker {
		public int Width { get; private set; }
		public int Height { get; private set; }

		List<Node> Nodes = new List<Node>();

		public RectanglePacker(int Width, int Height) {
			Nodes.Add(new Node(0, 0, Width, Height));
		}

		public bool Pack(int W, int H, out int X, out int Y) {
			for (int i = 0; i < Nodes.Count; ++i) {
				if (W <= Nodes[i].W && H <= Nodes[i].H) {
					Node Node = Nodes[i];
					Nodes.RemoveAt(i);
					X = Node.X;
					Y = Node.Y;

					int R = X + W;
					int B = Y + H;

					Nodes.Add(new Node(R, Y, Node.Right - R, H));
					Nodes.Add(new Node(X, B, W, Node.Bottom - B));
					Nodes.Add(new Node(R, B, Node.Right - R, Node.Bottom - B));
					Width = Math.Max(Width, R);
					Height = Math.Max(Height, B);

					return true;
				}
			}

			X = 0;
			Y = 0;
			return false;
		}

		public struct Node {
			public int X;
			public int Y;
			public int W;
			public int H;

			public Node(int x, int y, int w, int h) {
				X = x;
				Y = y;
				W = w;
				H = h;
			}

			public int Right {
				get { return X + W; }
			}

			public int Bottom {
				get { return Y + H; }
			}
		}

		public struct Rect {
			public object Userdata;
			public int X;
			public int Y;
			public int W;
			public int H;

			public Rect(int x, int y, int w, int h) {
				Userdata = null;
				X = x;
				Y = y;
				W = w;
				H = h;
			}

			public int Area {
				get { return W * H; }
			}
		}
	}
}