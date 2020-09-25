<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output method="html" indent="yes"/>

  <xsl:param name="cssPath"></xsl:param>
  <xsl:param name="imagepath">
    <xsl:text>..\images\</xsl:text>
  </xsl:param>
  <xsl:template match="*">
    <div>
      <xsl:attribute name="style">
        <xsl:text>border:solid 1px red;</xsl:text>
      </xsl:attribute>
      <xsl:text>[Objeket uten style: </xsl:text>
      <xsl:value-of select="name()"/>
      <xsl:text> Class: </xsl:text>
      <xsl:value-of select="@class"/>
      <xsl:text> Type: </xsl:text>
      <xsl:value-of select="@type"/>
      <xsl:text> Parent: </xsl:text>
      <xsl:value-of select="local-name(parent::*)" />
      <xsl:text> ]</xsl:text>
      <xsl:copy-of select="."/>
    </div>
  </xsl:template>

  <xsl:template match="/">
    <html>
      <link>
        <xsl:attribute name="href">
          <xsl:value-of select="$cssPath"/>
          <xsl:text>user.css</xsl:text>
        </xsl:attribute>
        <xsl:attribute name="rel">
          <xsl:text>stylesheet</xsl:text>
        </xsl:attribute>
        <xsl:attribute name="type">
          <xsl:text>text/css</xsl:text>
        </xsl:attribute>
        <xsl:attribute name="media">
          <xsl:text>all</xsl:text>
        </xsl:attribute>
      </link>
      <div>
        <xsl:attribute name="class">
          <xsl:text>lTopicText p3</xsl:text>
        </xsl:attribute>
        <xsl:apply-templates   />
      </div>
    </html>
  </xsl:template>

  <xsl:template match="level">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="title">
    <h2>
      <xsl:apply-templates />
    </h2>
  </xsl:template>

  <xsl:template match="text">
    <xsl:copy-of select="." />
  </xsl:template>
  
</xsl:stylesheet>