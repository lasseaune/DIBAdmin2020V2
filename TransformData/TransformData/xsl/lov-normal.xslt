<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:strip-space elements="*"/>

  <xsl:variable name="no_alfa" select="abcdefghijklmnopqrstuvwxyzæøå" />

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

  <xsl:template match="lovcontent">
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

  <xsl:template match="text">
    <xsl:copy-of select="." />
  </xsl:template>
  
  <xsl:template match="mirrors">
    <html>
    <table border="1">
      <xsl:apply-templates />
    </table>
    </html>
  </xsl:template>

  <xsl:template match="mirror">
    <tr>
      <xsl:if test="string(@state)!='id'">
        <xsl:attribute name="style">
          <xsl:text>background-color:silver;</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates />
    </tr>
  </xsl:template>

  <xsl:template match="mirror1|mirror2">
    <td width="50%" valign="top" align="left">
      <xsl:choose>
        <xsl:when test="content">
          <xsl:apply-templates />    
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>-----INGEN SAMSVAR--</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
      
    </td>
  </xsl:template>

  <xsl:template match="content">
    <xsl:apply-templates />
  </xsl:template>



  <xsl:template match="document">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="metadata" />

  <xsl:template match="title">
    <a>
      <xsl:attribute name="id" >
        <xsl:value-of select="../@id"/>
      </xsl:attribute>
      <xsl:attribute name="name" >
        <xsl:value-of select="../@id"/>
      </xsl:attribute>
    </a>
    <xsl:choose>
      <xsl:when test="parent::*/@type='lov'">
        <h2>
          <xsl:apply-templates select="t" />
        </h2>
      </xsl:when>
      <xsl:when test="parent::*/@type='vedlegg' or parent::*/@type='vedl' or parent::*/@type='del'">
        <h3>
          <xsl:apply-templates select="t" />
        </h3>
      </xsl:when>

      <xsl:when test="parent::*/@type='kapittel' or parent::*/@type='kap' or parent::*/@type='textpara' or parent::*/@type='text'">
        <h3>
          <xsl:apply-templates  select="t" />
        </h3>
      </xsl:when>
      <xsl:when test="parent::*/@type='del' or parent::*/@type='romer'">
        <h4>
          <xsl:apply-templates  select="t" />
        </h4>
      </xsl:when>
      <xsl:when test="parent::*/@type='lovpara' or parent::*/@type='para' or parent::*/@type='metric'  or parent::*/@type='center'">
        <h5>
          <xsl:apply-templates  select="t" />
        </h5>
      </xsl:when>
      <xsl:when test="parent::*/@type='artpara' or parent::*/@type='art'">
        <h5>
          <xsl:apply-templates  select="t" />
        </h5>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="parent::*/@level='1'">
            <h1>
              <xsl:apply-templates select="t" />
            </h1>
          </xsl:when>
          <xsl:when test="parent::*/@level='2'">
            <h2>
              <xsl:apply-templates select="t" />
            </h2>
          </xsl:when>
          <xsl:when test="parent::*/@level='3'">
            <h3>
              <xsl:apply-templates select="t" />
            </h3>
          </xsl:when>
          <xsl:when test="parent::*/@level='4'">
            <h4>
              <xsl:apply-templates select="t" />
            </h4>
          </xsl:when>
          <xsl:when test="parent::*/@level='5'">
            <h5>
              <xsl:apply-templates select="t" />
            </h5>
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


  <xsl:template match="uth[@type='italic']">
    <i>
      <xsl:apply-templates />
    </i>
  </xsl:template>


  <xsl:template match="a">
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
      <xsl:for-each select="t | a">
        <p>
          <xsl:apply-templates />
        </p>
      </xsl:for-each>
    </div>
  </xsl:template>


  <xsl:template match="section">
    <div>
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
      <xsl:attribute name="class">
        <xsl:text>normaltable</xsl:text>
      </xsl:attribute>

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
          <xsl:if test="@colnum!=''">
          </xsl:if>
          <xsl:if test="@colname!=''">
            <xsl:attribute name="name">
              <xsl:value-of select="@colname"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="@colwidth!=''">

            <xsl:variable name="width">
              <xsl:value-of select="translate(@colwidth, '*', '')"/>
            </xsl:variable>
            <xsl:if test="$width &gt; 0">
              <xsl:attribute name="width">
                <xsl:value-of select="$width" />
                <xsl:text>px</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:if>
          <xsl:if test="@colsep!=''">
          </xsl:if>
          <xsl:if test="@rowseP!=''">
          </xsl:if>
          <xsl:if test="@align!=''">
            <xsl:attribute name="align">
              <xsl:value-of select="@align"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:apply-templates />
        </col>
      </xsl:for-each>
    </colgroup>
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="row">
    <tr>
      <xsl:apply-templates />
    </tr>
  </xsl:template>

  <xsl:template match="entry">
    <td>
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
    </xsl:choose>

  </xsl:template>

  <xsl:template match ="bilde">

  </xsl:template>

  <xsl:template match="weblink">

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

  <xsl:template match="table[@type='asis']">
    <table>
      <xsl:if test="tr[1]/td[1]/@width='04%' or tr[1]/td[1]/@width='09%' or tr[1]/td[1]/@width='08%' or tr[1]/td[1]/@width='12%'">
        <xsl:attribute name="class">
          <xsl:text>tbNoBorder</xsl:text>
        </xsl:attribute>
      </xsl:if>
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

  <!--table section start-->
  <xsl:template match="table">
    <table>
      <xsl:call-template name="CopyElement" />
    </table>
  </xsl:template>

  <xsl:template match="thead">
    <thead>
      <xsl:call-template name="CopyElement" />
    </thead>
  </xsl:template>

  <xsl:template match="th">
    <th>
      <xsl:call-template name="CopyElement" />
    </th>
  </xsl:template>

  <xsl:template match="tr">
    <tr>
      <xsl:call-template name="CopyElement" />
    </tr>
  </xsl:template>

  <xsl:template match="td">
    <td>
      <xsl:call-template name="CopyElement" />
    </td>
  </xsl:template>
  <!--table section end-->

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

  <xsl:template match="p">
    <p>
      <xsl:apply-templates />
    </p>
  </xsl:template>

  <!--lagt til av lau 10/6-->

  <xsl:template match="a[@class='xref']">
    <a>
      <xsl:attribute name="class">
        <xsl:text>xref</xsl:text>
      </xsl:attribute>
      <xsl:call-template name="CopyElement" />
    </a>
  </xsl:template>

  <xsl:template match="span[@class='hn xreflist']">
    <xsl:copy-of select="." />
  </xsl:template>

</xsl:stylesheet>