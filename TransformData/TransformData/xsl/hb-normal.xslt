<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:preserve-space elements="t sup"/>
  <xsl:strip-space elements="p td title"/>

  <xsl:variable name="no_alfa" select="'abcdefghijklmnopqrstuvwxyzæøå'" />


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

  <xsl:template match="hbcontent">
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




  <xsl:template match="content">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="contentbox">
    <table width="100%" class="tbNoBorder" >
      <tr>
        <td valign="middle" align="left">
          <xsl:apply-templates />
        </td>
      </tr>
    </table>
  </xsl:template>


  <!--<xsl:template match="documents">

    <html>
      <link rel="stylesheet" type="text/css" href="content.css" />
      <link rel="stylesheet" type="text/css" href="table.css" />
      <link rel="stylesheet" type="text/css" href="global.css" />
      <body>
        <xsl:apply-templates />
      </body>
    </html>

  </xsl:template>-->


  <xsl:template match="document">


    <xsl:apply-templates />

  </xsl:template>

  <xsl:template match="title">
    <a>
      <xsl:attribute name="name" >
        <xsl:value-of select="../@id"/>
      </xsl:attribute>
      <xsl:attribute name="id" >
        <xsl:value-of select="../@id"/>
      </xsl:attribute>
    </a>

    <xsl:choose>
      <xsl:when test="not(string(parent::*/@dibt)='')">
        <h2>
          <xsl:apply-templates />
        </h2>
        <xsl:if test="parent::*/@decorated='underlined'">
          <hr/>
        </xsl:if>
      </xsl:when>
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
          <xsl:when test="@type='eksempeloverskrift'">
            <p>
              <xsl:attribute name="class">
                <xsl:value-of select="@type"/>
              </xsl:attribute>

              <xsl:apply-templates />
            </p>
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
              <xsl:when test="parent::*/@type='footnotes' or parent::*/@type='vedlegg' or parent::*/@type='vedl' or parent::*/@type='del' or parent::*/@type='ms1' or parent::*/@type='2'">
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
      <xsl:when test="@type='eksempeltekst'">
        <p>
          <xsl:attribute name="class">
            <xsl:value-of select="@type"/>
          </xsl:attribute>
          <xsl:apply-templates />
        </p>
      </xsl:when>
      <xsl:otherwise>
        <p>
          <xsl:apply-templates />
        </p>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="t">
    <xsl:choose>
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
          <xsl:if test="string(@type)!=''">
            <xsl:attribute name="class">
              <xsl:choose>
                <xsl:when test="@type='i'">
                  <xsl:text>italic</xsl:text>
                </xsl:when>
                <xsl:when test="@type='b'">
                  <xsl:text>bold</xsl:text>
                </xsl:when>
                <xsl:when test="@type='u'">
                  <xsl:text>underline</xsl:text>
                </xsl:when>

              </xsl:choose>
            </xsl:attribute>
          </xsl:if>
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

  <xsl:template match="section[@dibt='npara']">
    <table>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:text>tbNoBorder  mb3 mt3</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="width">
        <xsl:text>100%</xsl:text>
      </xsl:attribute>
      <tr>
        <td>
          <xsl:attribute name="width">
            <xsl:text>04%</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="align">
            <xsl:text>right</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="valign">
            <xsl:text>top</xsl:text>
          </xsl:attribute>
          <xsl:value-of select="@paranum"/>
        </td>
        <td>
          <xsl:attribute name="align">
            <xsl:text>left</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="valign">
            <xsl:text>top</xsl:text>
          </xsl:attribute>
          <xsl:apply-templates />
        </td>

      </tr>
    </table>
  </xsl:template>


  <xsl:template match="list">
    <xsl:call-template name="GetListPkt">
      <xsl:with-param name="level" select="0" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="GetListPkt">
    <xsl:param name="list" select="." />
    <xsl:param name ="level" />


    <xsl:for-each select="$list/pkt">
      <p>
        <xsl:attribute name="class">
          <xsl:text>normalnumber</xsl:text>
        </xsl:attribute>

        <xsl:attribute name="style">
          <xsl:text>margin-left:</xsl:text>
          <xsl:value-of select="18 + (18 * $level)"/>
          <xsl:text>pt;</xsl:text>
          <xsl:text>;text-indent:-18pt;</xsl:text>
        </xsl:attribute>

        <xsl:choose>
          <xsl:when test="parent::*/@type = 'ALFA'">
            <xsl:value-of select="substring('ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ', position(),1)"/>
            <xsl:text>.</xsl:text>
            <span>
              <xsl:attribute name="class">
                <xsl:text>buffer</xsl:text>
              </xsl:attribute>
              <xsl:text disable-output-escaping="yes">&#160;&#160;&#160;&#160;&#160;&#160;</xsl:text>
            </span>
          </xsl:when>
          <xsl:when test="parent::*/@type = 'alfa'">
            <xsl:value-of select="substring('abcdefghijklmnopqrstuvwxyzæøå', position(),1)"/>
            <xsl:text>.</xsl:text>
            <span>
              <xsl:attribute name="class">
                <xsl:text>buffer</xsl:text>
              </xsl:attribute>
              <xsl:text disable-output-escaping="yes">&#160;&#160;&#160;&#160;&#160;&#160;</xsl:text>
            </span>
          </xsl:when>
          <xsl:when test="parent::*/@type = 'strek'">
            <xsl:text>-</xsl:text>
            <span>
              <xsl:attribute name="class">
                <xsl:text>buffer</xsl:text>
              </xsl:attribute>
              <xsl:text disable-output-escaping="yes">&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;</xsl:text>
            </span>
          </xsl:when>
          <xsl:when test="parent::*/@type = 'num' or parent::*/@type = 'nummer'">
            <xsl:value-of select="position()"/>
            <xsl:text>.</xsl:text>

            <span>
              <xsl:attribute name="class">
                <xsl:text>buffer</xsl:text>
              </xsl:attribute>
              <xsl:choose>
                <xsl:when test="position() &lt; 10">
                  <xsl:text disable-output-escaping="yes">&#160;&#160;&#160;&#160;&#160;&#160;</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text disable-output-escaping="yes">&#160;&#160;</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </span>
          </xsl:when>
        </xsl:choose>


        <xsl:call-template name="GetPkt">
          <xsl:with-param name="level" select="$level + 1" />
        </xsl:call-template>
      </p>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="GetPkt">
    <xsl:param name="level" />
    <xsl:for-each select="child::node()">
      <xsl:choose>
        <xsl:when test="self::text()">
          <!--show text-->
          <xsl:value-of select="."/>
        </xsl:when>
        <xsl:when test="self::*">
          <!--an element-->
          <xsl:choose>
            <xsl:when test="local-name()='list'">
              <xsl:call-template name="GetListPkt">
                <xsl:with-param name="level" select="$level + 1" />
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="local-name()='a'">
              <xsl:apply-templates select="*" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates />
            </xsl:otherwise>

          </xsl:choose>
        </xsl:when>

      </xsl:choose>
    </xsl:for-each>
    <!--
    <xsl:for-each select="child::*">
      <xsl:choose>
        <xsl:when test="local-name()='list'">
          <xsl:call-template name="GetListPkt">
            <xsl:with-param name="level" select="$level + 1" />
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="local-name()='a'">
          <xsl:apply-templates select="*" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates />
        </xsl:otherwise>

      </xsl:choose>
    </xsl:for-each>
-->
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

  <xsl:template match="entry | td">
    <td>
      <xsl:attribute name ="class">
        <xsl:text>normaltd</xsl:text>
      </xsl:attribute>
      <xsl:call-template name="CopyElement" />
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


  <xsl:template match="center">
    <center>
      <xsl:call-template name="CopyElement" />
    </center>
  </xsl:template>

  <xsl:template match ="bilde">

  </xsl:template>

  <xsl:template match="weblink">

  </xsl:template>

  <!--
  <xsl:template match="xref | zref">
    <a>
      <xsl:attribute name="class">
        <xsl:text>basic</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:text>#</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="onclick">
        <xsl:text>TopicPanel.OpenXref(this, '</xsl:text>
        <xsl:value-of select="@rid"/>
        <xsl:text>');return false;</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </a>
  </xsl:template>
  -->

  <xsl:template match="xref[string(@rid)='']">

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

  <xsl:template match="yref">
    <a>
      <xsl:attribute name="class">
        <xsl:text>xref</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:text>#</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="data-assoc">
        <xsl:value-of select="@rid"/>
      </xsl:attribute>
      <xsl:attribute name="onclick">
        <xsl:text>return false;</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
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
  <xsl:template match="kws" />


  <xsl:template match="kw" />


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
      <xsl:text>)  </xsl:text>
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

  <xsl:template match="comment">
    <div>
      <xsl:attribute name="class">
        <xsl:value-of select="local-name()"/>
      </xsl:attribute>
      <xsl:for-each select="a">
        <p>
          <xsl:apply-templates />
        </p>
      </xsl:for-each>

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

  <xsl:template match="footnote[@type='fnifrs']">
    <sup>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:value-of select="local-name()"/>
      </xsl:attribute>
      <xsl:attribute name="title">
        <xsl:for-each select="descendant::t">
          <xsl:value-of select="."/>
        </xsl:for-each>
      </xsl:attribute>

      <a>
        <xsl:text>[*]</xsl:text>
      </a>


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
    <span>
      <xsl:attribute name="style">
        <xsl:text>font-size: 0.8em;position:relative;top:-7px</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>

  <xsl:template match="sub">
    <span>
      <xsl:attribute name="style">
        <xsl:text>font-size: 0.8em;position:relative;top:5px</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
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
    <xsl:variable name="nodename" select="name()" />
    <xsl:for-each select="@*">
      <xsl:if test="name() != 'idx' and name() != 'asis' and name() != 'class'">
        <xsl:if test="not($nodename='table' and name()='align')">
          <xsl:choose>
            <xsl:when test="name()='type'">
              <xsl:if test=".!='word'">
                <xsl:attribute name="class">
                  <xsl:choose>
                    <xsl:when test=".='olist'">
                      <xsl:text>tbNoBorder</xsl:text>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="."/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:attribute>
              </xsl:if>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="{name()}">
                <xsl:value-of select="."/>
              </xsl:attribute>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </xsl:if>
    </xsl:for-each>
    <xsl:apply-templates />
  </xsl:template>

  <!--Lagt til Lasse Aune 04 02 2010 -->
  <xsl:template match="dl">
    <dl>
      <xsl:apply-templates />
    </dl>
  </xsl:template>

  <xsl:template match="dt">
    <dt>
      <xsl:apply-templates />
    </dt>
  </xsl:template>

  <xsl:template match="dd">
    <dd>
      <xsl:apply-templates />
    </dd>
  </xsl:template>

  <xsl:template match="table[ancestor::document[1]/@type='origin' or ancestor::section[1]/@origin='word']">
    <table>
      <xsl:call-template name="CopyElement" />
    </table>
  </xsl:template>

  <xsl:template match="tr[ancestor::document[1]/@type='origin' or ancestor::section[1]/@origin='word']">
    <tr>
      <xsl:call-template name="CopyElement" />
    </tr>
  </xsl:template>

  <xsl:template match="td[ancestor::document[1]/@type='origin' or ancestor::section[1]/@origin='word']">
    <td>
      <xsl:call-template name="CopyElement" />
    </td>
  </xsl:template>

  <xsl:template match="bull">
    <xsl:text disable-output-escaping="yes">&amp;bull;&amp;nbsp;</xsl:text>
    <xsl:text> </xsl:text>
  </xsl:template>

  <!--Lasse Aune 20110330-->
  <xsl:template match="definition">
    <span>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:text>b</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>

  <xsl:template match="randlist">
    <ul>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="type">
        <xsl:text>disc</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </ul>
  </xsl:template>


  <xsl:template match="seqlist">
    <ol>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="type">
        <xsl:choose>
          <xsl:when test="@type='upper-alpha'">
            <xsl:text>A</xsl:text>
          </xsl:when>
          <xsl:when test="@type='lower-alpha'">
            <xsl:text>a</xsl:text>
          </xsl:when>
          <xsl:when test="@type='upper-roman'">
            <xsl:text>I</xsl:text>
          </xsl:when>
          <xsl:when test="@type='lower-roman'">
            <xsl:text>i</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>1</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:apply-templates />
    </ol>
  </xsl:template>

  <xsl:template match="item[parent::seqlist] | item[parent::randlist]" >
    <li>
      <xsl:apply-templates />
    </li>
  </xsl:template>

  <xsl:template match="toc">
    <table>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:text>tbNoBorder</xsl:text>
      </xsl:attribute>

      <col align="left" width="04%" />
      <col align="left" width="08%" />
      <col align="left" />
      <col align="right" width ="20%" />

      <thead>
        <tr>
          <th colspan="4">
            <xsl:value-of select="@toc_label"/>
          </th>
          <th>
            <xsl:value-of select="@paragraph_label"/>
          </th>
        </tr>
      </thead>

      <xsl:apply-templates />
    </table>
  </xsl:template>

  <xsl:template match="toc_entry[@level='primary'] | toc_entry[@level='introduction'] | toc_entry[@level='appendices']">
    <tr>
      <td colspan="4">
        <xsl:apply-templates select="toc_title"/>
      </td>
      <td>
        <xsl:apply-templates select="toc_paragraphs"/>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="toc_entry[@level='secondary']">
    <tr>
      <td></td>
      <td colspan="3">
        <xsl:apply-templates select="toc_title"/>
      </td>
      <td>
        <xsl:apply-templates select="toc_paragraphs"/>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="toc_entry[@level='tertiary']">
    <tr>
      <td></td>
      <td></td>
      <td colspan="2">
        <xsl:apply-templates select="toc_title"/>
      </td>
      <td>
        <xsl:apply-templates select="toc_paragraphs"/>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="toc_entry[@level='quaternary']">
    <tr>
      <td></td>
      <td></td>
      <td></td>
      <td>
        <span>
          <xsl:attribute name="class">
            <xsl:value-of select="@level"/>
          </xsl:attribute>
          <xsl:apply-templates select="toc_title"/>
        </span>
      </td>
      <td>
        <xsl:apply-templates select="toc_paragraphs"/>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="toc_title">
    <xsl:choose>
      <xsl:when test="@target">
        <a>
          <xsl:attribute name="class">
            <xsl:text>xref</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="href">
            <xsl:text>#</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="data-bm">
            <xsl:value-of select="@target"/>
          </xsl:attribute>
          <xsl:attribute name="onclick">
            <xsl:text>return false;</xsl:text>
          </xsl:attribute>
          <xsl:apply-templates />
        </a>

      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates />
      </xsl:otherwise>


    </xsl:choose>
  </xsl:template>

  <xsl:template name="para">
    <p>
      <xsl:apply-templates />
    </p>
  </xsl:template>

  <xsl:template match="toc_paragraphs">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="linebreak">
    <br/>
  </xsl:template>

  <xsl:template match="rubric">
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

  <xsl:template match="deflist">
    <a>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
    </a>
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="deflistitem">
    <table>
      <xsl:attribute name="width">
        <xsl:text>100%</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:text>tbNoBorder</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <tr>
        <td>
          <xsl:attribute name="width">
            <xsl:text>30%</xsl:text>
          </xsl:attribute>
          <xsl:apply-templates select="term" />
        </td>
        <td>
          <xsl:apply-templates select="def" />
        </td>
        <td>
          <xsl:attribute name="width">
            <xsl:text>20%</xsl:text>
          </xsl:attribute>
          <xsl:apply-templates select="gref" />
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template match="term[parent::deflistitem]">
    <span>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:text>bold</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>

  <xsl:template match="def[parent::deflistitem]">
    <div>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:apply-templates />
    </div>
  </xsl:template>

  <xsl:template match="gref[parent::deflistitem]">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="definition">
    <span>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:text>bold</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>

  <xsl:template match="boardmembers">
    <table>
      <xsl:attribute name="class">
        <xsl:text>tbNoBorder</xsl:text>
      </xsl:attribute>

      <xsl:apply-templates  />

    </table>
  </xsl:template>

  <xsl:template match="boardmember">
    <tr>
      <td>
        <xsl:apply-templates select="name"/>
      </td>
      <td>
        <xsl:apply-templates select="role"/>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="name[parent::boardmember]">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="role[parent::boardmember]">
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
        <xsl:text>dibimages/</xsl:text>
        <xsl:value-of select="@folder"/>
        <xsl:text>/</xsl:text>
        <xsl:value-of select="@file"/>
      </xsl:attribute>
    </img>
  </xsl:template>

  <xsl:template match="t[@type='del'] | deletedtext">
    <span>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:text>tline</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>

  <xsl:template match="t[@type='ins'] | insertedtext">
    <span>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:text>changed</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>

  <xsl:template match="xref[@type='ifrs']">
    <a>

      <xsl:attribute name="class">
        <xsl:text>xref</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:text>#</xsl:text>
      </xsl:attribute>
      <!--segm_id
				doc_id
				sec_id
				obj_id
				-->
      <xsl:attribute name="data-doc">
        <xsl:value-of select="@doc_id"/>
      </xsl:attribute>
      <xsl:attribute name="data-bm">
        <xsl:value-of select="@sec_id"/>
      </xsl:attribute>
      <xsl:attribute name="onclick">
        <xsl:text>return false;</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </a>

  </xsl:template>

  <xsl:template match="xref[@type='ifrsdl']">
    <a>
      <xsl:attribute name="class">
        <xsl:text>xref def</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="href">
        <xsl:text>#</xsl:text>
      </xsl:attribute>
      <!--segm_id
				doc_id
				sec_id
				obj_id
				-->
      <xsl:attribute name="data-bm">
        <xsl:value-of select="@obj_id"/>
      </xsl:attribute>
      <xsl:attribute name="onclick">
        <xsl:text>return false;</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </a>

  </xsl:template>

  <xsl:template match="linebreak">
    <br/>
  </xsl:template>


  <xsl:template match="a[@class='xref']">
    <a>
      <xsl:attribute name="class">
        <xsl:text>xref</xsl:text>
      </xsl:attribute>
      <xsl:call-template name="CopyElement" />
    </a>
  </xsl:template>

  <xsl:template match="edu_insert">
    <span>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:text>tEdu</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </span>
  </xsl:template>

  <xsl:template match="edu_para">
    <div>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:text>tEdu</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </div>
  </xsl:template>

  <xsl:template match="edu_para">
    <div>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:text>tEdu</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates />
    </div>
  </xsl:template>

  <xsl:template match="edu_para[preceding-sibling::*/@dibt='npara' or following-sibling::*/@dibt='npara']">
    <table>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:text>tbNoBorder mb3 mt3</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="width">
        <xsl:text>100%</xsl:text>
      </xsl:attribute>
      <tr>
        <td>
          <xsl:attribute name="width">
            <xsl:text>04%</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="align">
            <xsl:text>right</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="valign">
            <xsl:text>top</xsl:text>
          </xsl:attribute>

        </td>
        <td>
          <xsl:attribute name="class">
            <xsl:text>tEdu</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="align">
            <xsl:text>left</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="valign">
            <xsl:text>top</xsl:text>
          </xsl:attribute>
          <xsl:apply-templates />
        </td>

      </tr>
    </table>
  </xsl:template>

  <!--Lagt til 31/8-2011 Lasse Aune for IFRSEY2011-->

  <xsl:template match="table[@type='olist' or @type='word']">
    <table>
      <xsl:call-template name="CopyElement" />
    </table>
  </xsl:template>

  <xsl:template match="tr[ancestor::table[1]/@type='olist' or ancestor::table[1]/@type='word']">
    <tr>
      <xsl:call-template name="CopyElement" />
    </tr>
  </xsl:template>

  <xsl:template match="td[ancestor::table[1]/@type='olist' or ancestor::table[1]/@type='word']">
    <td>
      <xsl:call-template name="CopyElement" />
    </td>
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
      <!--segm_id
				doc_id
				sec_id
				obj_id
				-->
      <xsl:attribute name="data-bm">
        <xsl:value-of select="@ref"/>
      </xsl:attribute>
      <xsl:attribute name="onclick">
        <xsl:text>return false;</xsl:text>
      </xsl:attribute>
      <sup>
        <xsl:apply-templates />
      </sup>
      <!--
	  <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="fid">
        <xsl:value-of select="@ref"/>
      </xsl:attribute>
      <sup>
        <xsl:apply-templates />
      </sup>
	  -->
    </a>
  </xsl:template>

  <xsl:template match="graphic[@type='img' and string(@large)='']">
    <p>
      <img>
        <xsl:if test="string(@width)!=''">
          <xsl:attribute name="width">
            <xsl:value-of select="@width"/>
          </xsl:attribute>
        </xsl:if>

        <xsl:if test="string(@height)!=''">
          <xsl:attribute name="height">
            <xsl:value-of select="@height"/>
          </xsl:attribute>
        </xsl:if>

        <xsl:if test="string(@src)!=''">
          <xsl:attribute name="src">
            <xsl:text>dibimages/</xsl:text>
            <xsl:value-of select="@folder"/>
            <xsl:text>/</xsl:text>
            <xsl:value-of select="@src"/>
          </xsl:attribute>
        </xsl:if>

        <xsl:if test="string(@large)!=''">
          <xsl:attribute name="data_orginal">
            <xsl:text>dibimages/</xsl:text>
            <xsl:value-of select="@folder"/>
            <xsl:text>/</xsl:text>
            <xsl:value-of select="@large"/>
            <xsl:text>/</xsl:text>
            <xsl:value-of select="@src"/>
          </xsl:attribute>
        </xsl:if>
      </img>
    </p>
  </xsl:template>
  <xsl:template match="graphic[@type='img' and string(@large)!='']">
    <p>
      <a>
        <xsl:attribute name="href">
          <xsl:text>dibimages/</xsl:text>
          <xsl:value-of select="@folder"/>
          <xsl:text>/</xsl:text>
          <xsl:value-of select="@large"/>
          <xsl:text>/</xsl:text>
          <xsl:value-of select="@src"/>
        </xsl:attribute>
        <xsl:attribute name="target">
          <xsl:text>_blank</xsl:text>
        </xsl:attribute>
        <img>
          <xsl:if test="string(@width)!=''">
            <xsl:attribute name="width">
              <xsl:value-of select="@width"/>
            </xsl:attribute>
          </xsl:if>

          <xsl:if test="string(@height)!=''">
            <xsl:attribute name="height">
              <xsl:value-of select="@height"/>
            </xsl:attribute>
          </xsl:if>

          <xsl:if test="string(@src)!=''">
            <xsl:attribute name="src">
              <xsl:text>dibimages/</xsl:text>
              <xsl:value-of select="@folder"/>
              <xsl:text>/</xsl:text>
              <xsl:value-of select="@src"/>
            </xsl:attribute>
          </xsl:if>

          <xsl:if test="string(@large)!=''">
            <xsl:attribute name="data_orginal">
              <xsl:text>dibimages/</xsl:text>
              <xsl:value-of select="@folder"/>
              <xsl:text>/</xsl:text>
              <xsl:value-of select="@large"/>
              <xsl:text>/</xsl:text>
              <xsl:value-of select="@src"/>
            </xsl:attribute>
          </xsl:if>
        </img>
      </a>
    </p>
  </xsl:template>

  <xsl:template match="span[@class='hn xreflist']">
    <xsl:copy-of select="." />
  </xsl:template>
  
</xsl:stylesheet>