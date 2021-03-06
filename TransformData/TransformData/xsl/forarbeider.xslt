﻿<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
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

  <xsl:template match="document">
    <xsl:apply-templates   />
  </xsl:template>

  <xsl:template match="section">
    <xsl:apply-templates   />
  </xsl:template>

  <xsl:template name ="GetParentId">
    <xsl:attribute name="id">
      <xsl:value-of select="../@id"/>
    </xsl:attribute>
  </xsl:template>

  <xsl:template name ="GetItemId">
    <xsl:if test="string(@id)!=''">
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>

  <xsl:template match="title">
    <xsl:variable name="n" select="count(ancestor::section)" />
    <xsl:choose>
      <xsl:when test="$n=1" >
        <h1>
          <xsl:call-template name="GetParentId" />
          <xsl:apply-templates  />
        </h1>
      </xsl:when>
      <xsl:when test="$n=2" >
        <h2>
          <xsl:call-template name="GetParentId" />
          <xsl:apply-templates  />
        </h2>
      </xsl:when>
      <xsl:when test="$n=3" >
        <h3>
          <xsl:call-template name="GetParentId" />
          <xsl:apply-templates  />
        </h3>
      </xsl:when>
      <xsl:otherwise>
        <h4>
          <xsl:call-template name="GetParentId" />
          <xsl:apply-templates  />
        </h4>
      </xsl:otherwise>
    </xsl:choose>
    </xsl:template>

  
  <xsl:template match="p">
    <p>
      <xsl:call-template name="GetItemId" />
      <xsl:apply-templates />
    </p>
  </xsl:template>

  <xsl:template match="p[@type!='']">
    <p>
      <xsl:choose>
        <xsl:when test="@type='k-tilrar' or @type='k-stadfester'">
          <xsl:attribute name="class">
            <xsl:text>tac ic</xsl:text>
          </xsl:attribute>
        </xsl:when>
      </xsl:choose>
      <xsl:apply-templates />
    </p>
  </xsl:template>


  <xsl:template match="mtit">
    <h4>
      <xsl:apply-templates />
    </h4>
  </xsl:template>

  <xsl:template match="i">
    <i>
      <xsl:apply-templates />
    </i>
  </xsl:template>

  <xsl:template match="u">
    <u>
      <xsl:apply-templates />
    </u>
  </xsl:template>

  <xsl:template match="b|strong">
    <b>
      <xsl:apply-templates />
    </b>
  </xsl:template>


  <xsl:template match="ol">
    <ol>
      <xsl:attribute name="style">
        <xsl:choose>
          <xsl:when test="@type='k-num'">
            <xsl:text>list-style-type: decimal;</xsl:text>
          </xsl:when>
          <xsl:when test="@type='k-rom'">
            <xsl:text>list-style-type:lower-roman;</xsl:text>
          </xsl:when>
          <xsl:when test="@type='k-alfa'">
            <xsl:text>list-style-type:lower-alpha;</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>list-style-type:none;</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:apply-templates />
    </ol>
  </xsl:template>



  <xsl:template match="ul">
    <ul>
      <xsl:attribute name="style">
        <xsl:choose>
          <xsl:when test="@type='k-opprams' or  @type='k-ingen' or @type='beslo-list-none'">
            <xsl:text>list-style-type:none;</xsl:text>
          </xsl:when>
          <xsl:when test="@type='innst-list-disc'">
            <xsl:text>list-style-type:disc;list-style-position: outside;</xsl:text>
          </xsl:when>
          <xsl:when test="@type='innst-list-lower-alpha' or @type='innst-list-lower-decimal' or @type='innst-list-lower-roman'">
            <xsl:text>list-style-type:none;list-style-position: inside;margin-left: 2em;</xsl:text>
          </xsl:when>
          <xsl:when test="@type='k-strek' or @type='k-bombe' ">
            <xsl:text>list-style-type:disc;</xsl:text>
          </xsl:when>
          <xsl:when test="@type='k-annet' ">
            <xsl:text>list-style-type:none;</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>list-style-type:none;</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:apply-templates />
    </ul>
  </xsl:template>

  <xsl:template match="li[local-name(parent::*) != 'ul' and local-name(parent::*) != 'ol']">
    <li>
      <xsl:choose>
        <xsl:when test="@type='k-pkt-lovbokst' or @type='k-pkt-lovnr' or @type='beslo-list-none'">
          <xsl:attribute name="style">
            <xsl:text>margin:0em 0em 0em 1em;list-style-type: none;</xsl:text>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="style">
            <xsl:text>margin:0em 0em 0em 1em;list-style-type: none;</xsl:text>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:apply-templates />
    </li>
  </xsl:template>

  <xsl:template match="li[local-name(parent::*) = 'ul' or local-name(parent::*) = 'ol']">
    <li>
      <xsl:choose>
        <xsl:when test="@type='k-ingen' or parent::*/@type='k-opprams' or parent::*/@type='k-annet'  or parent::*/@type='beslo-list-none'">
          <xsl:attribute name="style">
            <xsl:text>margin : 0em 0em 0em 1em;list-style-type: none;</xsl:text>
          </xsl:attribute>
        </xsl:when>
        <xsl:when test="parent::*/@type='k-ingen' or parent::*/@type='k-opprams'">
          <xsl:attribute name="style">
            <xsl:text>margin : 0em 0em 0em 1em;list-style-type: none;</xsl:text>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="style">
            <xsl:text>margin :  0em 0em 0em 1em;</xsl:text>
            <!--<xsl:text>margin :  0.4em 0em 0em 0em;</xsl:text>-->
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:apply-templates />
    </li>
  </xsl:template>


  <xsl:template match="span">
    <span>
      <!--<xsl:attribute name="style">
      <xsl:choose>
        <xsl:when test="@type='k-inline-endring'">
          <xsl:text>font-style:italic ; margin-left : 0.3em;margin-right: 0.3em; </xsl:text>
        </xsl:when>
      </xsl:choose>
      </xsl:attribute>-->
      <xsl:apply-templates />
    </span>
  </xsl:template>
  <xsl:template match="ny-anf">
    <span>
      <xsl:attribute name="style">
        <xsl:text>font-weight:bold; </xsl:text>
      </xsl:attribute>
      <xsl:text>("</xsl:text>
      <xsl:apply-templates />
      <xsl:text>")</xsl:text>
    </span>
  </xsl:template>
  <xsl:template match="blokk[@type='ramme']">
    <table>
      <xsl:attribute name="class">
        <xsl:text>tbRubric</xsl:text>
      </xsl:attribute>
      <tr>
        <td>
          <xsl:apply-templates />
        </td>
      </tr>
    </table>
  </xsl:template>
  <xsl:template match="blokk[@type='fig']">
    <xsl:apply-templates />
  </xsl:template>
  <xsl:template match="blokk[@type='medlem']">
    <xsl:apply-templates />
  </xsl:template>
  <xsl:template match="blokk[@type='sitat']">
    <blockquote>
      <xsl:apply-templates />
    </blockquote>
  </xsl:template>
  <xsl:template match="footnote[@type='sectionnote']">
    <a>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>

      <xsl:attribute name="class">
        <xsl:text>xref </xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:text>#</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="data-bm">
        <xsl:value-of select="@ref"/>
      </xsl:attribute>
      <xsl:attribute name="onclick">
        <xsl:text>return false;</xsl:text>
      </xsl:attribute>
      <sup>
        <xsl:apply-templates />
      </sup>
    </a>
  </xsl:template>
  <xsl:template match="uth">
    <xsl:choose>
      <xsl:when test="@type='u'">
        <span>
          <xsl:attribute name="class">
            <xsl:text>underline</xsl:text>
          </xsl:attribute>
          <xsl:apply-templates />
        </span>
      </xsl:when>
      <xsl:when test="@type='b' or @type='k-uthevet'">
        <span>
          <xsl:attribute name="class">
            <xsl:text>b</xsl:text>
          </xsl:attribute>
          <xsl:apply-templates />
        </span>
      </xsl:when>
      <xsl:when test="@type='k' or @type='i' or @type='italic'">
        <span>
          <xsl:attribute name="class">
            <xsl:text>italic</xsl:text>
          </xsl:attribute>
          <xsl:apply-templates />
        </span>
      </xsl:when>
      <xsl:when test="@type='highlight'">
        <span>
          <xsl:attribute name="class">
            <xsl:value-of select="@type"/>
          </xsl:attribute>
          <xsl:apply-templates />
        </span>
      </xsl:when>
      <xsl:otherwise>
        <span>
          <xsl:apply-templates />
        </span>
      </xsl:otherwise>

    </xsl:choose>

  </xsl:template>
  <xsl:template match="table">
    <table>
      <xsl:apply-templates />
    </table>
  </xsl:template>
  <xsl:template match="thead">
    <thead>
      <xsl:apply-templates />
    </thead>
  </xsl:template>
  <xsl:template match="tbody">
    <tbody>
      <xsl:apply-templates />
    </tbody>
  </xsl:template>

  <xsl:template match="tr">
    <tr>
      <xsl:apply-templates />
    </tr>
  </xsl:template>

  
  <xsl:template match="td">
    <td>
      <xsl:apply-templates />
    </td>
  </xsl:template>

  <xsl:template match="th">
    <th>
      <xsl:apply-templates />
    </th>
  </xsl:template>

  <xsl:template match="blokk[@type='k-del-tilrpost' or @type='k-del-kongeside']">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="blockquote">
    <blockquote>
      <xsl:apply-templates />
    </blockquote>
  </xsl:template>

  <xsl:template match="cite">
    <cite>
      <xsl:apply-templates />
    </cite>
  </xsl:template>
  <xsl:template match="em">
    <em>
      <xsl:apply-templates />
    </em>
  </xsl:template>
  <xsl:template match="ledd">
      <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="graphic">
    <img>
      <xsl:attribute name="align">
        <xsl:text>middle</xsl:text>
      </xsl:attribute>

      <xsl:attribute name="border">
        <xsl:text>0</xsl:text>
      </xsl:attribute>

      <xsl:attribute name="alt">
        <xsl:value-of select="alt_text"/>
      </xsl:attribute>

      <xsl:attribute name="src">
        <xsl:value-of select="$imagepath"/>
        <xsl:value-of select="@file"/>
      </xsl:attribute>
    </img>
  </xsl:template>
  
  <xsl:template match="xref | zref">
    <a>
      <xsl:attribute name="class">
        <xsl:text>xref</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:text>#</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="data-bm">
        <xsl:value-of select="@rid"/>
      </xsl:attribute>
      <xsl:attribute name="onclick">
        <xsl:text>return false;</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </a>
  </xsl:template>
  <xsl:template match="text">
    <xsl:copy-of select="." />
  </xsl:template>
  <!--<xsl:template match="text">
    <div>
      <xsl:attribute name="style">
        <xsl:text>border:solid 1px green;</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </div>
  </xsl:template>-->

  <xsl:template match="a[@class='xref']">
    <xsl:copy-of select="."/>
  </xsl:template>

</xsl:stylesheet>
