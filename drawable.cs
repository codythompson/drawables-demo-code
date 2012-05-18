/*
 * The Drawable abstract class is used by the flatverse engine to represent
 * anything that can be drawn to the screen.
 * 
 * This file contains the Drawable class definition, as well as some basic
 * sub classes of the Drawable class.
 */

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using flatverse;

namespace flatverse.Drawables
{
    /*
     * An abstract class used by the engine as an interface for anything
     * that can be drawn to the screen.
     */
    public abstract class Drawable
    {
        //the scale, color, depth layer, and pixel offset of a drawable object
        public Vector2 scale;
        public Color color;
        public float layer;
        public Vector2 offset;

        /*
         * instantiates a Drawable object with default values
         */
        public Drawable()
        {
            color = Color.White;
            scale = new Vector2(1, 1);

            layer = 0.5f;

            offset = Vector2.Zero;
        }

        /*
         * instantiates a Drawable object with the given values
         */
        public Drawable(Vector2 scale, Color color, float layer)
        {
            this.scale = scale;
            this.color = color;
            this.layer = layer;

            offset = Vector2.Zero;
        }

        /*
         * Tells the drawable to update itself
         * 
         * should return true if an animation has finished
         * false otherwise
         */
        public abstract bool update();

        /*
         * Draws the Drawable -
         * spriteBatch - the SpriteBatch object to draw with
         * px - the location (in pixels) to draw this object to the screen at
         * scale - a scale to apply to the drawable (should be multiplied by 
         *         the instance var called scale)
         * color/colorAmount - the color to blend the object with and the
         *                     amount to blend it by.
         */
        public abstract void draw(SpriteBatch spriteBatch, Vector2 px,
            Vector2 scale, Color color, float colorAmount);

        /*
         * returns a copy of this instance of drawable
         */
        public abstract Drawable clone();
    }

    /*
     * A Drawable that draws a static image to the screen.
     */
    public class DStatic : Drawable
    {
        //the image to draw and the part of the image to draw.
        public Texture2D image;
        public Rectangle src;

        /*
         * Instantiates an instance of DStatic
         * 
         * image - the image to draw to the screen
         * src - the part of the image to draw
         * scale - a scale multiplier to be applied to the image
         * color - a color modifier to be applied to the image
         * layer - the depth layer that the image should be drawn at
         */
        public DStatic(Texture2D image, Rectangle src, Vector2 scale, 
            Color color, float layer)
            : base(scale, color, layer)
        {
            this.image = image;
            this.src = src;
        }

        /// <summary>
        /// This extending Drawable class has no need for the update method.
        /// This method will always return true.
        /// </summary>
        /// <returns>true always.</returns>
        public override bool update()
        {
            return false;
        }

        /*
         * Draws the image to the screen.
         * 
         * spriteBatch - the SpriteBatch object to draw to the screen with
         * px - the location, in pixels, to draw the image at on screen
         * scale - a scale multiplier that is first multiplied by the instance
         *         var called scale, than applied to the image
         * color/colorAmount - A color modifier that is blended with the
         *                     intance var called color using Linear
         *                     Interpolation
         */
        public override void draw(SpriteBatch spriteBatch, Vector2 px, 
            Vector2 scale, Color color, float colorAmount)
        {
            Color c = this.color;
            if (colorAmount != 0) //if the colorAmount != 0 perform a LERP
            {
                c = Color.Lerp(this.color, color, colorAmount);
            }
            spriteBatch.Draw(image, px + offset, src, c, 0.0f, Vector2.Zero,
                this.scale * scale, SpriteEffects.None, layer);
        }

        /*
         * Returns a copy of this object
         */
        public override Drawable clone()
        {
            DStatic nd = new DStatic(image, src, scale, color, layer);
            nd.color = color;
            nd.scale = scale;
            nd.layer = layer;
            nd.offset = offset;
            return nd;
        }
    }

    /*
     * Draws a line to the screen by scaling a texture (that should be 1 pixel
     * wide) 
     */
    public class DStaticLine : Drawable
    {
        //the angle of the line
        public float angle; //radians
        //represents the length and angle of the line
        private Vector2 length_angle;

        //the 1 pixel wide image that is scaled and rotated to look like a line
        public Texture2D image;
        public Rectangle src;

        /*
         * Intantiates a new DStaticLine object
         * 
         * length_angle - represents the length/angle of the line
         * image - a 1 pixel wide image that will be scaled and rotated
         *         to go from the position being draw at to the position
         *         being drawn at + length_angle.
         *  src - the part of the image to draw aka Source Rectangle
         *  color - the color modifier for the image
         *  layer - the depth layer to draw the image at.
         */
        public DStaticLine(
            Vector2 length_angle, Texture2D image,
            Rectangle src, Color color, float layer)
            : base(new Vector2(length_angle.Length(), 1), color, layer)
        {
            angle = (float)Math.Atan2(length_angle.Y, length_angle.X);
            this.length_angle = length_angle;

            this.image = image;
            this.src = src;
        }

        /*
         * no need to update - not animated
         * will always return false.
         */
        public override bool update()
        {
            return false;
        }

        /*
         * Draws the line
         * 
         * spriteBatch - used to draw the line
         * px - the location on screen to start drawing the line at
         * scale - a scale multiplier
         * color/colorAmount - blended with this objects intance var called
         *                     color using Color.LERP
         */
        public override void draw(SpriteBatch spriteBatch, Vector2 px,
            Vector2 scale, Color color, float colorAmount)
        {
            Color c = this.color;
            if (colorAmount != 0) //if colorAmount != 0 perform a LERP
            {
                c = Color.Lerp(this.color, color, colorAmount);
            }
            spriteBatch.Draw(image, px + offset, src, c, angle,
                Vector2.Zero, this.scale * scale, SpriteEffects.None, layer);
        }

        /*
         * returns a copy of this Drawable
         */
        public override Drawable clone()
        {
            DStaticLine nd = new DStaticLine(length_angle, image, src, color, layer);
            nd.scale = scale;
            nd.offset = offset;
            return nd;
        }
    }

    /*
     * Draws a string to the screen
     */
    public class DString : Drawable
    {
        //the font to draw the string with
        public SpriteFont font;

        //the string to draw
        public string str;

        /*
         * Instantiates a new DString object
         */
        public DString(SpriteFont font, string str)
        {
            this.font = font;
            this.str = str;
        }

        /*
         * not animated - will always return true
         */
        public override bool update()
        {
            return false;
        }

        /*
         * Draws the string
         * 
         * spriteBatch - the object to draw the string with
         * px - the position on screen (in pixels) to draw the string at
         * scale - a scale modifier to be applied to the graphical string
         * color/colorAmount - will be blended with this objects existing color
         *                     field value using Color.LERP
         */
        public override void draw(SpriteBatch spriteBatch, Vector2 px,
            Vector2 scale, Color color, float colorAmount)
        {
            Color c = this.color;
            if (colorAmount != 0) //if colorAmount != 0 perform a LERP
            {
                c = Color.Lerp(this.color, color, colorAmount);
            }
            spriteBatch.DrawString(font, str, px, c, 0, Vector2.Zero,
                this.scale * scale, SpriteEffects.None, layer);
        }

        /*
         * returns a copy of this object
         */
        public override Drawable clone()
        {
            return new DString(font, str);
        }
    }
}
