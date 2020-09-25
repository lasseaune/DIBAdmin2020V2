<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="html" indent="yes"/>

    <xsl:template match="/">
      <html>
        <table>
          <tr>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
          </tr>
          <xsl:for-each select="section">
            <xsl:call-template name="GetSections" />
          </xsl:for-each>
        </table>
      </html>
      </xsl:template>

  <xsl:template name="GetSections">
    <tr>
      <td>
        <xsl:attribute name="colspan">
          <xsl:value-of select="@level"/>
        </xsl:attribute>
      </td>
      <td>
        <xsl:if test="string(@from)=''">
          <xsl:attribute name="style">
            <xsl:text>background-color:red;</xsl:text>
          </xsl:attribute>

        </xsl:if>
        <xsl:attribute name="colspan">
          <xsl:value-of select="9 -  @level"/>
        </xsl:attribute>
        <xsl:value-of  select="@text"/>
        <xsl:if test="string(@id)!=''">
          <span>
            <xsl:attribute name="style">
              <xsl:text>color:red;</xsl:text>
            </xsl:attribute>
          <xsl:text> (</xsl:text>
          <xsl:value-of  select="@id"/>
          <xsl:text>)</xsl:text>
          </span>
        </xsl:if>
      </td>
    </tr>
    <xsl:for-each select="child::section">
      <xsl:call-template name="GetSections" />
    </xsl:for-each>
  </xsl:template>
    
</xsl:stylesheet>
