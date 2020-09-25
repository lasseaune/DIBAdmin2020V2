<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt">
  <xsl:output method="html"  indent="yes"/>
  <xsl:preserve-space elements="*"/>

  <xsl:variable name="no_alfa" select="abcdefghijklmnopqrstuvwxyzæøå" />

  

  <xsl:template match="/">
    <html>
      <link rel="stylesheet" type="text/css" href="../css/lov.css" />
      <link rel="stylesheet" type="text/css" href="../css/global.css" />
      <body>
        
    <xsl:apply-templates />
      </body>
    </html>
  </xsl:template>


  <xsl:template match="root">
    <xsl:apply-templates select="documents" />
  </xsl:template>


  <xsl:template match="documents">
    <xsl:call-template name="CreateStyle" />
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="*"/>
  
  <xsl:template match="content">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="document">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="title">
    <a>
      <xsl:attribute name="name" >
        <xsl:value-of select="../@id"/>
      </xsl:attribute>
    </a>
   
    <xsl:choose>
      
      <xsl:when test="not(string(@type)='')">
        <xsl:choose>
          <xsl:when test="@type='stikkt' or @type='ms' or @type='tit'">
            <h1>
              <xsl:apply-templates />
            </h1>
          </xsl:when>
          <xsl:when test="@type='ms1'">
            <h2>
              <xsl:apply-templates />
            </h2>
          </xsl:when>
          <xsl:when test="@type='ms2'">
            <h3>
              <xsl:apply-templates />
            </h3>
          </xsl:when>
          <xsl:when test="@type='ms3'">
            <h4>
              <xsl:apply-templates />
            </h4>
          </xsl:when>
          <xsl:when test="@type='ms4'">
            <h5>
              <xsl:apply-templates />
            </h5>
          </xsl:when>
          <xsl:when test="@type='ms5'">
            <h6>
              <xsl:apply-templates />
            </h6>
          </xsl:when>
          <xsl:when test="@type='ms6'">
            <h7>
              <xsl:apply-templates />
            </h7>
          </xsl:when>
          <xsl:when test="@type='enkeltsaker-tittel'">
            <p>
              <xsl:attribute name="class">
                <xsl:value-of select="@type"/>
              </xsl:attribute>

              <xsl:apply-templates />
            </p>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="not(string(parent::*/@type)='')">
            <xsl:choose>
              <xsl:when test="parent::*/@type='1'" >
                <h1>
                  <xsl:apply-templates select="t" />
                </h1>
              </xsl:when>
              <xsl:when test="parent::*/@type='2'" >
                <h2>
                  <xsl:apply-templates select="t" />
                </h2>
              </xsl:when>

              <xsl:when test="parent::*/@type='5'" >
                <h5>
                  <xsl:apply-templates select="t" />
                </h5>
              </xsl:when>
              <xsl:when test="parent::*/@type='6'" >
                <h5>
                  <xsl:apply-templates select="t" />
                </h5>
              </xsl:when>
              <xsl:when test="parent::*/@type='lov' or parent::*/@type='sek' or parent::*/@type='1'">
                <h1>
                  <xsl:apply-templates select="t" />
                </h1>
              </xsl:when>
              <xsl:when test="parent::*/@type='vedlegg' or parent::*/@type='vedl' or parent::*/@type='del' or parent::*/@type='ms1' or parent::*/@type='2'">
                <h2>
                  <xsl:apply-templates select="t" />
                </h2>
              </xsl:when>

              <xsl:when test="parent::*/@type='kapittel' or parent::*/@type='kap' or parent::*/@type='textpara' or parent::*/@type='text'  or parent::*/@type='ms2'">
                <h2>
                  <xsl:apply-templates  select="t" />
                </h2>
              </xsl:when>
              <xsl:when test="parent::*/@type='del' or parent::*/@type='romer' or parent::*/@type='ms3' or parent::*/@type='3'">
                <h3>
                  <xsl:apply-templates  select="t" />
                </h3>
              </xsl:when>
              <xsl:when test="parent::*/@type='lovpara' or parent::*/@type='para' or parent::*/@type='metric'  or parent::*/@type='center' or parent::*/@type='ms4' or parent::*/@type='4'">
                <h4>
                  <xsl:apply-templates  select="t" />
                </h4>
              </xsl:when>
              <xsl:when test="parent::*/@type='artpara' or parent::*/@type='art'">
                <h4>
                  <xsl:apply-templates  select="t" />
                </h4>
              </xsl:when>

            </xsl:choose>

          </xsl:when>
          <xsl:otherwise>
            <h3>
              <xsl:apply-templates select="t" />
            </h3>
          </xsl:otherwise>
        </xsl:choose>

      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template match="a | p">
    <xsl:choose>
      <xsl:when test="not(@type)">
        <p>
          <xsl:attribute name="class">
            <xsl:text>normal</xsl:text>
          </xsl:attribute>
          <xsl:apply-templates />
        </p>
      </xsl:when>

      <xsl:when test="@type='a'">
        <p>
          <xsl:attribute name="class">
            <xsl:text>normal</xsl:text>
          </xsl:attribute>
          <xsl:apply-templates />
        </p>
      </xsl:when>
      <xsl:when test="@type='enkeltsaker-1'">
        <p>
          <xsl:attribute name="class">
            <xsl:value-of select="@type"/>
          </xsl:attribute>
          <xsl:apply-templates />
        </p>
      </xsl:when>
      <xsl:when test="@type='enkeltsaker-2'">
        <p>
          <xsl:attribute name="class">
            <xsl:value-of select="@type"/>
          </xsl:attribute>
          <xsl:apply-templates />
        </p>
      </xsl:when>

    </xsl:choose>
  </xsl:template>

  <xsl:template match="t">
    <xsl:choose>
      <xsl:when test="@type">
        <span>
          <xsl:attribute name="id">
            <xsl:value-of select="@id"/>
          </xsl:attribute>

          <xsl:attribute name="class">
            <xsl:value-of select="@type"/>
          </xsl:attribute>
          <xsl:apply-templates />
        </span>

      </xsl:when>
      <xsl:when test="@state='changed'">
        <span>
          <xsl:attribute name="id">
            <xsl:value-of select="@id"/>
          </xsl:attribute>
          
          <xsl:attribute name="class">
            <xsl:value-of select="@state"/>
          </xsl:attribute>
          <xsl:apply-templates />
        </span>
      </xsl:when>
      <xsl:otherwise>
        <span>
          <xsl:attribute name="id">
            <xsl:value-of select="@id"/>
          </xsl:attribute>
          <xsl:apply-templates />
        </span>
        
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="para | section[@type='para']" >
    <div>
      <xsl:attribute name="class">
        <xsl:value-of select="@type"/>
      </xsl:attribute>
      <xsl:apply-templates />
    </div>
  </xsl:template>

  <xsl:template match="section">
    <div>
      <xsl:attribute name="idx">
        <xsl:value-of select="@idx"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:value-of select="local-name()"/>
      </xsl:attribute>
      <xsl:apply-templates />
    </div>
  </xsl:template>

  <xsl:template match="list">

    <xsl:choose>
      <xsl:when test="@type = 'strek'">
        <ul>
          <xsl:attribute name="type">
            <xsl:text>square</xsl:text>
          </xsl:attribute>
          <xsl:apply-templates select="pkt" />
        </ul>
      </xsl:when>
      <xsl:otherwise>
        <ol>
          <xsl:attribute name="type">
            <xsl:choose>
              <xsl:when test="@type='loweralfa'">
                <xsl:text>a</xsl:text>
              </xsl:when>
              <xsl:when test="@type='upperalfa'">
                <xsl:text>A</xsl:text>
              </xsl:when>
              <xsl:when test="@type='lowerroman'">
                <xsl:text>i</xsl:text>
              </xsl:when>
              <xsl:when test="@type='upperroman'">
                <xsl:text>I</xsl:text>
              </xsl:when>
              <xsl:when test="@type='number'">
                <xsl:text>1</xsl:text>
              </xsl:when>
            </xsl:choose>
          </xsl:attribute>
          <xsl:apply-templates select="pkt" />
        </ol>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template match="pkt">
    <li>
      <xsl:attribute name="value">
        <xsl:value-of select="@value"/>
      </xsl:attribute>
      <xsl:for-each select="a">
        <xsl:if test="position() &gt; 1">

          <br/>
          <br/>
          

        </xsl:if>
        <xsl:apply-templates />
      </xsl:for-each>
      <xsl:apply-templates select="list" />
    </li>  
  </xsl:template>

  <xsl:template name="GetA">
    <xsl:param name="a" select="." />
    <xsl:param name="level" />

    <xsl:for-each select="$a/child">
      <xsl:apply-templates />
    </xsl:for-each>
  </xsl:template>

  
  <xsl:template match="table">
    <table>
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="."/>
        </xsl:attribute>
      </xsl:for-each>
      <xsl:choose>
        <xsl:when test="not(string(@type))">
          <xsl:attribute name="class">
            <xsl:text>normaltable</xsl:text>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="class">
            <xsl:value-of select="@type"/>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>

      <xsl:if test="@frame!=''">
        <xsl:attribute name="frame">
          <xsl:choose>
            <xsl:when test="@frame='none'">
              <xsl:text>void</xsl:text>
            </xsl:when>
            <xsl:when test="@frame='topbot'">
              <xsl:text>hsides</xsl:text>
            </xsl:when>
            <xsl:when test="@frame='all'">
              <xsl:text>box</xsl:text>
            </xsl:when>
          </xsl:choose>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates />
    </table>
  </xsl:template>


  <xsl:template match="thead">
    <thead>
      <xsl:if test="@valign!=''">
        <xsl:attribute name="valign">
          <xsl:value-of select="@valign"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates />
    </thead>
  </xsl:template>

  <xsl:template match="tbody">
    <tbody>
      <xsl:if test="@valign!=''">
        <xsl:attribute name="valign">
          <xsl:value-of select="@valign"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates />
    </tbody>
  </xsl:template>

  <xsl:template match="tfoot">
    <tfoot>
      <xsl:if test="@valign!=''">
        <xsl:attribute name="valign">
          <xsl:value-of select="@valign"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates />
    </tfoot>
  </xsl:template>

  <xsl:template match="tgroup">
    <colgroup>
      <xsl:for-each select="colspec">
        <col>
          <xsl:call-template name="CopyElement" />
        </col>
      </xsl:for-each>
    </colgroup>
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="row | tr">
    <tr>
      <xsl:call-template name="CopyElement" />
    </tr>
  </xsl:template>

    
  <xsl:template match="entry | td ">
    <td>

      <xsl:if test="string(@colspan)!=''">
        <xsl:attribute name="colspan">
          <xsl:value-of select="@colspan"/>
        </xsl:attribute>
      </xsl:if>

      <xsl:attribute name ="class">
        <xsl:text>normaltd</xsl:text>
      </xsl:attribute>

      <xsl:if test="@colname!=''">
        <xsl:attribute name="name">
          <xsl:value-of select="@colname"/>
        </xsl:attribute>
      </xsl:if>

      <xsl:if test="@spanname!=''">
        <xsl:variable name ="currspan" select="@spanname" />

        <xsl:variable name="span">
          <xsl:value-of select="ancestor::tgroup/spanspec[@spanname=$currspan]"/>
        </xsl:variable>

        <xsl:if test="$span">
          <xsl:variable name="start" select="substring-before($span,'-')" />
          <xsl:variable name="slutt" select="substring-after($span,'-')" />

          <xsl:attribute name="colspan">
            <xsl:value-of select="($start - $slutt) + 1"/>
          </xsl:attribute>
        </xsl:if>
      </xsl:if>

      <xsl:if test="@colsep!=''">
        <xsl:variable name ="currspan" select="@colsep" />

        <xsl:variable name="span">
          <xsl:value-of select="ancestor::tgroup/spanspec[@spanname=$currspan]"/>
        </xsl:variable>

        <xsl:if test="$span">
          <xsl:variable name="start" select="substring-before($span,'-')" />
          <xsl:variable name="slutt" select="substring-after($span,'-')" />

          <xsl:attribute name="colspan">
            <xsl:value-of select="($start - $slutt) + 1"/>
          </xsl:attribute>
        </xsl:if>
      </xsl:if>

      <xsl:if test="@rowsep!=''">
      </xsl:if>

      <xsl:if test="@valign!=''">
        <xsl:attribute name="valign">
          <xsl:value-of select="valign" />
        </xsl:attribute>
      </xsl:if>

      <xsl:if test="@align!=''">
        <xsl:attribute name="align">
          <xsl:value-of select="@align"/>
        </xsl:attribute>
      </xsl:if>

      <xsl:if test="@SECURITY!=''">
      </xsl:if>


      <xsl:apply-templates />
    </td>
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
      <xsl:when test="@type='b'">
        <span>
          <xsl:attribute name="class">
            <xsl:text>bold</xsl:text>
          </xsl:attribute>
          <xsl:apply-templates />
        </span>
      </xsl:when>
      <xsl:when test="@type='k' or @type='i'">
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
    </xsl:choose>
  </xsl:template>

  <xsl:template match="uth[@type='italic']">
    <i>
      <xsl:apply-templates />
    </i>
  </xsl:template>

  <xsl:template match="center">
    <center>
      <xsl:call-template name="CopyElement" />
    </center>
  </xsl:template>


  <xsl:template match ="bilde">

  </xsl:template>

  <xsl:template match="weblink">

  </xsl:template>

  <xsl:template match="xref | zref">
    <a>
      <xsl:attribute name="href">
        <xsl:text>#</xsl:text>
        <xsl:value-of select="@rid"/>
      </xsl:attribute>

      <xsl:value-of select="." />
    </a>
  </xsl:template>

  <xsl:template match="wref">
    <a>
      <xsl:attribute name="class">
        <xsl:text>basic</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:value-of select="@rid"/>
      </xsl:attribute>
      <xsl:attribute name="target">
        <xsl:text>_blank</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </a>
  </xsl:template>

  <xsl:template match="brok">
    <table>
      <xsl:attribute name="class">
        <xsl:value-of select="local-name()"/>
      </xsl:attribute>
      <xsl:apply-templates />
    </table>
  </xsl:template>

  <xsl:template match="tel">
    <tr>
      <td>
        <xsl:attribute name="class">
          <xsl:value-of select="local-name()"/>
        </xsl:attribute>
        <xsl:apply-templates />
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="nev">
    <tr>
      <td>
        <xsl:attribute name="class">
          <xsl:value-of select="local-name()"/>
        </xsl:attribute>
        <xsl:apply-templates />
      </td>
    </tr>
  </xsl:template>
  <xsl:template match="kws">
    <p>
      <xsl:attribute name="class">
        <xsl:value-of select="local-name()"/>
      </xsl:attribute>
      <xsl:apply-templates />
    </p>
  </xsl:template>

  <xsl:template match="kw">
    <span>
      <xsl:attribute name="class">
        <xsl:value-of select="local-name()"/>
      </xsl:attribute>

      <a>
        <xsl:attribute name="id">
          <xsl:value-of select="@id"/>
        </xsl:attribute>

        <xsl:attribute name="name">
          <xsl:value-of select="@id"/>
        </xsl:attribute>

      </a>

      <xsl:apply-templates />
    </span>

  
  </xsl:template>

  <xsl:template match="ledd[@type='static']">
    <div>
      <xsl:attribute name="class">
        <xsl:value-of select="local-name()"/>
      </xsl:attribute>
      <xsl:apply-templates />
    </div>
  </xsl:template>

  <xsl:template match="ledd">
    <div>
      <xsl:attribute name="class">
        <xsl:value-of select="local-name()"/>
      </xsl:attribute>
      <xsl:text>(</xsl:text>
      <xsl:value-of select="@value"/>
      <xsl:text>)</xsl:text>
      <br/>
      <xsl:apply-templates />
    </div>
  </xsl:template>

  <xsl:template match="ledda">
    <div>
      <xsl:attribute name="class">
        <xsl:value-of select="local-name()"/>
      </xsl:attribute>
      <xsl:apply-templates />
    </div>
  </xsl:template>
  
  <!--Endret-->
  <xsl:template match="comment">
    <div>
      <xsl:attribute name="class">
        <xsl:value-of select="local-name()"/>
      </xsl:attribute>
          <xsl:apply-templates />
    </div>
  </xsl:template>

  <xsl:template match="quote">
    <div>
      <xsl:attribute name="class">
        <xsl:text>quote</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </div>
  </xsl:template>


  <xsl:template match="footnotelov">
    <sup>
      <xsl:attribute name="title">
        <xsl:value-of select="@title"/>
      </xsl:attribute>
        <xsl:apply-templates />
    </sup>
  </xsl:template>


  <xsl:template match="footnote1">
    <sup>
      <xsl:attribute name="fid">
        <xsl:value-of select="@fid"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:value-of select="local-name()"/>
      </xsl:attribute>
      <a>
        <xsl:attribute name="title">
          <xsl:value-of select="@title"/>
        </xsl:attribute>
        <xsl:apply-templates />
      </a>
    </sup>
  </xsl:template>

  <xsl:template match="footnote">
    <p>
      <xsl:attribute name="fid">
        <xsl:value-of select="@fid"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:value-of select="local-name()"/>
      </xsl:attribute>
      <xsl:apply-templates />
    </p>
  </xsl:template>

  <xsl:template match="sup">
    <sup>
      <xsl:apply-templates />
    </sup>
  </xsl:template>

  <xsl:template match="sub">
    <sub>
      <xsl:apply-templates />
    </sub>
  </xsl:template>

  <xsl:template match="ol">
    <ol>
      <xsl:attribute name="class">
        <xsl:value-of select="@type"/>
      </xsl:attribute>

      <xsl:apply-templates />
    </ol>
  </xsl:template>

  <xsl:template match="ul">
    <ul>
      <xsl:attribute name="class">
        <xsl:value-of select="@type"/>
      </xsl:attribute>

      <xsl:apply-templates />
    </ul>
  </xsl:template>

  <xsl:template match="li">
    <li>
      <xsl:attribute name="type">
        <xsl:value-of select="@type"/>
      </xsl:attribute>

      <xsl:apply-templates />
    </li>
  </xsl:template>

  <xsl:template match="strong">
    <strong>
      <xsl:apply-templates />
    </strong>
  </xsl:template>

  <xsl:template match="br">
    <br/>
  </xsl:template>

  <xsl:template name="CreateStyle">
    <xsl:if test="docstyle">
      <style>
        <xsl:for-each select="docstyle/docstyleobject">
          <xsl:value-of select="@name"/>
          <xsl:text> </xsl:text>
          <xsl:value-of select="@value"/>
        </xsl:for-each>
      </style>
    </xsl:if>
  </xsl:template>

  <xsl:template match="qs">
    <xsl:text>&#171;</xsl:text>
  </xsl:template>

  <xsl:template match="qe">
    <xsl:text>&#187;</xsl:text>
  </xsl:template>

  <xsl:template match="th">
    <th>

      <xsl:attribute name ="class">
        <xsl:text>normaltd</xsl:text>
      </xsl:attribute>

      <xsl:call-template name="CopyElement" />
    </th>
  </xsl:template>

  <xsl:template name="CopyElement">
    <xsl:for-each select="@*">
      <xsl:if test="name() != 'idx' and name() != 'asis' and name() != 'class'">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="."/>
        </xsl:attribute>
      </xsl:if>
    </xsl:for-each>
    <xsl:apply-templates />
  </xsl:template>



  <xsl:template match="p[@type='asis']">
    <p>
      <xsl:apply-templates />
    </p>
  </xsl:template>

  <xsl:template match="table[@type='asis']">
    <table>
      <xsl:attribute name="class">
        <xsl:text>asis</xsl:text>
      </xsl:attribute>
      <xsl:call-template name="CopyElement" />
    </table>
  </xsl:template>

  <xsl:template match="thead[ancestor::table/@type='asis']">
    <thead>
      <xsl:call-template name="CopyElement" />
    </thead>
  </xsl:template>

  <xsl:template match="th[ancestor::table/@type='asis']">
    <th>
      <xsl:call-template name="CopyElement" />
    </th>
  </xsl:template>

  <xsl:template match="tr[ancestor::table/@type='asis']">
    <tr>
      <xsl:call-template name="CopyElement" />
    </tr>
  </xsl:template>

  <xsl:template match="td[ancestor::table/@type='asis']">
    <td>
      <xsl:call-template name="CopyElement" />
    </td>
  </xsl:template>

  <xsl:template match="xsource[@type='ltptoken']">
    <span>
      <xsl:attribute name="class">
        <xsl:text>ltptoken</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>


  <xsl:template match="xsource[@type='pldtoken']">
    <span>
      <xsl:attribute name="class">
        <xsl:text>pldtoken</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>


  <xsl:template match="xsource[@type='plttoken']">
    <span>
      <xsl:attribute name="class">
        <xsl:text>plttoken</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>


  
  <xsl:template match="xsource[@type='pltoken']">
    <span>
      <xsl:attribute name="class">
        <xsl:text>pltoken</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>

  <xsl:template match="xsource[@type='pdennefortoken']">
    <span>
      <xsl:attribute name="class">
        <xsl:text>pdennefortoken</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>

  <xsl:template match="xsource[@type='pdennelovtoken']">
    <span>
      <xsl:attribute name="class">
        <xsl:text>pdennelovtoken</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>


  <xsl:template match="xsource[@type='denneforptoken']">
    <span>
      <xsl:attribute name="class">
        <xsl:text>denneforptoken</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>


  <xsl:template match="xsource[@type='dennelovptoken']">
    <span>
      <xsl:attribute name="class">
        <xsl:text>dennelovptoken</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>

  <xsl:template match="xsource[@type='lt2ptoken']">
    <span>
      <xsl:attribute name="class">
        <xsl:text>lt2ptoken</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>

  <xsl:template match="xsource[@type='lt2token']">
    <span>
      <xsl:attribute name="class">
        <xsl:text>lt2token</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>

  <xsl:template match="xsource[@type='pftoken']">
    <span>
      <xsl:attribute name="class">
        <xsl:text>pftoken</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>


  <xsl:template match="xsource[@type='ldptoken']">
    <span>
      <xsl:attribute name="class">
        <xsl:text>ldptoken</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>


  <xsl:template match="xsource[@type='stoken']">
    <span>
      <xsl:attribute name="class">
        <xsl:text>stoken</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
   </xsl:template>

  <xsl:template match="xsource[@type='lovdate']">
    <span>
      <xsl:attribute name="class">
        <xsl:text>lovdate</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>

  <xsl:template match="xsource[@type='lovname']">
    <span>
      <xsl:attribute name="class">
        <xsl:text>lovname</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>


  <xsl:template match="xparas">
    <span>
      <xsl:attribute name="class">
        <xsl:text>xparas</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>

  <xsl:template match="xpara">
    <span>
      <xsl:attribute name="class">
        <xsl:choose>
          <xsl:when test="parent::*/@type ='internal'">
            <xsl:text>xparai</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>xpara</xsl:text>
          </xsl:otherwise>
        </xsl:choose>

      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>

</xsl:stylesheet>
