/*
 * Contains sub classes of Drawable that perform tiled drawing
 * and animations
 */

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace flatverse.Drawables
{
    //an interface that allows a Drawable to be resettable
    public interface Resettable
    {
        void reset();
    }

    /*
     * A Drawable subclass that splits an image into tiles
     */
    public class DTextureSheet : Drawable
    {
        //the source rectangles of the individual tiles
        public Rectangle[] srcRects;
        //the image containing the tiles
        public Texture2D image;

        //the index of the src rectangle of the current tile to be drawn
        private int i;

        //the initial index of the initial source rectangle to use
        private int initialIx;

        /*
         * either returns the current index
         * 
         * or sets the current index - changing the value
         * if it is less than zero or greater the length of srcRects - 1
         * to avoid indexOutOfBoundsExceptions
         */
        public int ix
        {
            get
            {
                return i;
            }
            set
            {
                if (value < 0)
                    i = 0;
                else if (value >= srcRects.Length)
                    i = srcRects.Length - 1;
                else
                    i = value;
            }
        }

        /*
         * Instantiates a new instance of DTextureSheet
         * 
         * image - an image that will be split into tiles
         * srcRects - the locations on the image of the individual tiles
         * initialIx - the index of the source rectangle to use first
         */
        public DTextureSheet(Texture2D image, Rectangle[] srcRects,
            int initialIx, float layer) :
            base(Vector2.One, Color.White, layer)
        {
            this.image = image;
            this.srcRects = srcRects;
            ix = initialIx;
            this.initialIx = initialIx;
        }

        /*
         * this Drawable is not animated
         * - always returns false
         */
        public override bool update()
        {
            return false;
        }

        /*
         * Draws the part of image that is defined by srcRects[ix] to screen
         * 
         * px - the location to draw the tile at
         * scale - stretches/shrinks the image
         * color/colorAmount - LERPed with this.color
         */
        public override void draw(SpriteBatch spriteBatch, Vector2 px,
            Vector2 scale, Color color, float colorAmount)
        {
            Color c = this.color;
            if (colorAmount != 0)
            {
                c = Color.Lerp(this.color, color, colorAmount);
            }
            spriteBatch.Draw(image, px + offset, srcRects[i], c, 0,
                Vector2.Zero, scale * this.scale, SpriteEffects.None, layer);
        }

        /*
         * increments the index that specifies which source rectangle to use
         * 
         * returns whether or not the end of srcRects has been reached
         */
        public virtual bool inc()
        {
            i++;
            if (i == srcRects.Length) //rollover if at the end of srcRects
            {
                i = 0;
                return true;
            }
            return false;
        }

        /*
         * The following methods are convenience methods for 1x1 to 3x3
         * tiled sprite sheets.
         * 
         * They will return the source rectangle at the location implied
         * by the method name.
         */
        public virtual Rectangle getTopLeft()
        {
            return srcRects[0];
        }
        public virtual Rectangle getTop()
        {
            if (srcRects.Length == 1)
                return srcRects[0];
            return srcRects[1];
        }
        public virtual Rectangle getTopRight()
        {
            if (srcRects.Length == 1)
                return srcRects[0];
            if (srcRects.Length == 2)
                return srcRects[1];
            return srcRects[2];
        }
        public virtual Rectangle getLeft()
        {
            if (srcRects.Length < 4)
                return getTopLeft();
            return srcRects[3];
        }
        public virtual Rectangle getMiddle()
        {
            if (srcRects.Length < 5)
                return getTop();
            return srcRects[4];
        }
        public virtual Rectangle getRight()
        {
            if (srcRects.Length < 6)
                return getTopRight();
            return srcRects[5];
        }
        public virtual Rectangle getBottomLeft()
        {
            if (srcRects.Length < 7)
                return getLeft();
            return srcRects[6];
        }
        public virtual Rectangle getBottom()
        {
            if (srcRects.Length < 8)
                return getMiddle();
            return srcRects[7];
        }
        public virtual Rectangle getBottomRight()
        {
            if (srcRects.Length < 9)
                return getRight();
            return srcRects[8];
        }

        /*
         * static convenience method for getting the source rectangles for the
         * given image.
         * 
         * textureSheet - an image that can be split into even width/height tiles
         * tilesWide - the number of tiles in the X direction that 
         *             textureSheet contains
         * tilesHigh - the number of tiles in the Y direction that 
         *             textureSheet contains
         */
        public static Rectangle[] getSplitSrcRects(Texture2D textureSheet,
            int tilesWide, int tilesHigh)
        {
            Rectangle[] sRects = new Rectangle[tilesWide * tilesHigh];

            int tWidth = textureSheet.Width / tilesWide;
            int tHeight = textureSheet.Height / tilesHigh;

            for (int i = 0; i < tilesWide * tilesHigh; i++)
            {
                int x = (i % tilesWide) * tWidth;
                int y = (i / tilesWide) * tHeight;
                sRects[i] = new Rectangle(x, y, tWidth, tHeight);
            }

            return sRects;
        }

        /*
         * returns a copy of this object
         */
        public override Drawable clone()
        {
            DTextureSheet nd = new DTextureSheet(image, srcRects, initialIx, layer);
            nd.color = color;
            nd.scale = scale;
            nd.layer = layer;
            nd.offset = offset;
            return nd;
        }
    }

    /*
     * An animated sprite sheet
     * 
     * Uses DTextureSheet to display a portion of an image for
     * some amount of time, then displays a different portion of
     * the image.
     * If the image is formatted correctly, this class will animate
     * the portions/tiles of the image.
     */
    public class DAnimated : DTextureSheet, Resettable
    {
        //how close the obj is to the end of the animation
        private double prog;
        //the amount to change by per update.
        private double delt;

        //an array containing indices of source rectangles
        public int[] pattern;

        //whether or not this animation should loop
        public bool loop;

        //the time it takes for one animation to cycle through
        private double duration;

        /*
         * Intantiates a new DAnimated instance
         * 
         * image - the image that contains the animation
         * srcRects - the srcRectangles of the frames of the animation
         * pattern - contains srcRects indices in the order that the
         *           frames should appear in.
         * duration - the time it should take for one animation to happen.
         * layer - the depth layer to draw the animation at.
         */
        public DAnimated(Texture2D image, Rectangle[] srcRects,
            int[] pattern, double duration, float layer) :
            base(image, srcRects, pattern[0], layer)
        {
            prog = 0;
            delt = 1 / (duration * GLOBALS.fps);

            this.pattern = pattern;

            loop = true;

            this.duration = duration;
        }

        /// <summary>
        /// Updates the animation.
        /// Assumes will be called flatverse.GLOBALS.fps times per second
        /// </summary>
        /// <returns>
        /// true - if the animation has finished
        /// false - otherwise
        /// </returns>
        public override bool update()
        {
            prog += delt;

            //if the animation is finsihed
            if (prog > 1)
            {
                if (loop) //if loop is true just reset progress
                    prog = 0;
                else //else set the frame to the last frame
                     //and return true;
                {
                    prog = 1;
                    ix = pattern[pattern.Length - 1];
                    return true;
                }
            }

            //grab's the srcRect index based on prog
            ix = pattern[(int)(prog * pattern.Length)];
            return false;
        }

        /*
         * helper methods for getting source rect indices
         * to be used with the constructor of this class
         */
        public static int[] getRange(int exclusiveEnd)
        {
            int[] range = new int[exclusiveEnd];

            for (int iter = 0; iter < range.Length; iter++)
            {
                range[iter] = iter;
            }

            return range;
        }

        public static int[] getRange(int inclusiveStart, int exclusiveEnd)
        {
            int[] range = new int[exclusiveEnd - inclusiveStart];
            for (int i = 0; i < range.Length; i++)
            {
                range[i] = i + inclusiveStart;
            }
            return range;
        }

        public static int[] getShuttleRange(int exclusiveEnd)
        {
            int[] range = new int[exclusiveEnd * 2];

            int iter;
            for (iter = 0; iter < (range.Length / 2); iter++)
            {
                range[iter] = iter;
            }
            int ix = iter;
            for (iter = iter - (range.Length % 2) - 1; iter > 0; iter--)
            {
                range[ix] = iter;
                ix++;
            }

            return range;
        }

        public static int[] getShuttleRange(int tilesWide, int tilesHigh)
        {
            return getShuttleRange(tilesWide * tilesHigh);
        }

        /*
         * returns a copy of this object
         */
        public override Drawable clone()
        {
            DAnimated nd = new DAnimated(image, srcRects, pattern, duration, layer);
            nd.color = color;
            nd.scale = scale;
            nd.layer = layer;
            nd.offset = offset;
            return nd;
        }

        /*
         * resets this animation so it will start over again.
         */
        public virtual void reset()
        {
            prog = 0;
        }
    }
}